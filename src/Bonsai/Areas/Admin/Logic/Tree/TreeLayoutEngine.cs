using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Code.Services.Graphviz;
using Bonsai.Data.Models;
using Newtonsoft.Json.Linq;

namespace Bonsai.Areas.Admin.Logic.Tree;

/// <summary>
/// Calculates the layout of a family tree.
///
/// Graphviz only ever decides where the cards go: it is handed a graph of family
/// blocks (a couple merged into one wide node), and everything else - the gaps
/// inside a row, the choice of which spouse stands on which side, the union
/// points and every wire - is worked out here. dot has no notion of "keep these
/// two nodes adjacent", which is why the couples are merged before layout.
/// </summary>
public class TreeLayoutEngine
{
    public TreeLayoutEngine(IGraphvizService graphviz)
    {
        _graphviz = graphviz;
    }

    private readonly IGraphvizService _graphviz;

    #region Constants

    private const double Spacing = 30;      // margin around the whole canvas
    private const double RankSep = 0.75;    // dot units (inches); 0.42 starts producing crossings
    private const double NodeSep = 0.56;

    // The vertical band between two rows of cards, and how the wires divide it up.
    // Every horizontal run in the band - the marriage brackets, the sibling buses,
    // and the lanes a trunk steps into to get past a row - gets a shelf of its own
    // one LaneStep from the next, otherwise runs collapse into one another and read
    // as noise, or worse, as a line that means something (see LaneMap). A band deep
    // enough for one bracket shelf, one bus shelf and the clearance between them is
    // exactly the RowGap dot leaves; ExpandRows opens up the ones that need more.
    private static readonly double RowGap = JsRound(RankSep * 72);
    private static readonly double LaneStep = JsRound(RowGap / 3);
    private static readonly double UnionDrop = LaneStep;                // card bottom -> union point
    private const double CardClearance = 20;                            // sideways room when dodging a card

    // A row wants two different minimum gaps, and dot's nodesep is a single number
    // for the whole graph. So dot lays the row out at the tight one and Spread()
    // opens up the pairs that deserve the wide one.
    private static readonly double KinGap = JsRound(NodeSep * 72);      // between siblings: keep the group tight
    private static readonly double StrangerGap = KinGap * 2;            // between families with no sibling link

    // dot's result depends on the order the nodes and edges are declared in, and the
    // spread between a lucky and an unlucky order is large (25% on total edge length
    // on the reference tree). So the layout is run many times over shuffled orders
    // and the best-scoring one is kept.
    // This only pays off for the full tree: the partial trees (ancestors, descendants,
    // close family) are narrow and near-hierarchical, dot gets them right on the first
    // order, and they are rendered once per page - so they run a single attempt.
    private const int MinAttempts = 8;          // always try at least this many orders
    private const int MaxAttempts = 400;
    private const int BudgetMs = 2000;          // stop shuffling once this much time has been spent
    private const int BatchSize = 25;           // candidates handed to dot per invocation
    private const int CompactPasses = 24;       // sweeps of the slide-into-the-slack pass
    private const double WidthWeight = 8;       // px of edge length one px of extra width is worth

    #endregion

    #region Per-run card geometry

    // The card geometry of the two views. Everything about a row is measured in
    // these numbers, so switching the view is a matter of setting them before the
    // layout runs: dot gets smaller nodes and every gap expressed in card terms
    // follows along. The compact card is half as wide, a little shorter, and keeps
    // its couple gap and marriage ports in proportion to that.
    private double _cardWidth;      // card box, as the front-end will render it
    private double _cardHeight;
    private double _coupleGap;      // gap between two spouses inside one family block
    private double _portInset;      // keeps the marriage ports off the card corners

    /// <summary>
    /// Sets the card geometry for the requested view.
    /// </summary>
    private void ApplyView(TreeViewMode view)
    {
        if (view == TreeViewMode.Compact)
            (_cardWidth, _cardHeight, _coupleGap, _portInset) = (150, 76, 30, 20);
        else
            (_cardWidth, _cardHeight, _coupleGap, _portInset) = (300, 100, 60, 40);
    }

    #endregion

    #region Entry point

    /// <summary>
    /// Calculates the layout of a tree and returns it as the JSON the front-end reads.
    /// </summary>
    /// <param name="data">Tree contents.</param>
    /// <param name="exhaustive">
    /// Search over many declaration orders and keep the tidiest layout, instead of
    /// taking dot's first result as it comes.
    /// </param>
    /// <param name="direction">Whether the parents go on top or at the bottom.</param>
    /// <param name="view">Card size.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task<string> LayoutAsync(TreeLayoutVM data, bool exhaustive, TreeDirection direction, TreeViewMode view, CancellationToken token)
    {
        ApplyView(view);

        // CanonicalPersons() is order-insensitive, so it gets the payload as it came;
        // everything downstream works off the id-sorted copy.
        var persons = CanonicalPersons(data.Persons ?? [], data.Relations ?? []);
        var relations = (data.Relations ?? []).OrderBy(x => x.Id, StringComparer.Ordinal).ToList();

        var blocks = BuildBlocks(persons, relations);
        var ctx = BuildContext(persons, relations, blocks);

        var pos = await SearchAsync(persons, relations, blocks, ctx, exhaustive, token);

        Normalize(pos);
        AlignUnions(pos, relations, ctx);

        // The wires are routed twice. The first pass runs on the layout as dot left
        // it and only hands out lane numbers, which is what tells us how many
        // horizontal runs each band between two rows has to hold; the rows are then
        // pushed apart far enough to give every run a lane of its own, and the second
        // pass draws the wires for real. A lane number depends only on the x range of
        // the run, and no x moves when rows slide down, so both passes agree on who
        // gets which lane.
        var flat = BuildBands(pos, persons);
        var probe = new LaneMap { Flat = true };
        RouteEdges(persons, relations, pos, flat, probe);
        ExpandRows(pos, persons, flat, probe);

        var bands = BuildBands(pos, persons);
        var edges = RouteEdges(persons, relations, pos, bands, new LaneMap());
        var size = Measure(pos);

        var tree = new JObject
        {
            ["id"] = "root",
            ["width"] = size.Width,
            ["height"] = size.Height,
            ["children"] = BuildChildren(persons, relations, pos),
            ["edges"] = edges
        };

        if (direction == TreeDirection.BottomToTop)
            FlipVertical(tree, size.Height);

        return tree.ToString(Newtonsoft.Json.Formatting.None);
    }

    #endregion

    #region Person ordering

    /// <summary>
    /// Sorts the persons into an order that does not depend on the ids.
    ///
    /// The backend invents a fresh random Guid for every "unknown spouse" placeholder
    /// on every run, so an id-derived order makes the layout differ from run to run
    /// for no reason. Sorting by name plus the surrounding names pins it down.
    /// </summary>
    private static List<TreePersonVM> CanonicalPersons(IReadOnlyList<TreePersonVM> persons, IReadOnlyList<TreeRelationVM> relations)
    {
        var name = new Dictionary<string, string>();
        foreach (var p in persons)
            name[p.Id] = p.Name ?? "";

        var spouses = new Dictionary<string, List<string>>();
        var kids = new Dictionary<string, List<string>>();

        foreach (var rel in relations)
        {
            Add(spouses, rel.From, name.GetValueOrDefault(rel.To) ?? "");
            Add(spouses, rel.To, name.GetValueOrDefault(rel.From) ?? "");
        }

        foreach (var p in persons)
        {
            if (p.Parents != null)
                Add(kids, p.Parents, p.Name ?? "");
        }

        var keyed = new List<(TreePersonVM Person, string Key, int Index)>();
        for (var i = 0; i < persons.Count; i++)
        {
            var person = persons[i];

            var own = spouses.TryGetValue(person.Id, out var mine)
                ? string.Join(",", mine.OrderBy(x => x, StringComparer.Ordinal))
                : "";

            var below = new List<string>();
            foreach (var rel in relations)
            {
                if (rel.From == person.Id || rel.To == person.Id)
                {
                    if (kids.TryGetValue(rel.Id, out var list))
                        below.AddRange(list);
                }
            }
            below.Sort(StringComparer.Ordinal);

            var key = (person.Name ?? "") + (person.Parents ?? "") + own + string.Join(",", below);
            keyed.Add((person, key, i));
        }

        keyed.Sort((a, b) =>
        {
            var byKey = string.CompareOrdinal(a.Key, b.Key);
            return byKey != 0 ? byKey : a.Index - b.Index;
        });

        return keyed.Select(x => x.Person).ToList();
    }

    private static void Add<T>(Dictionary<string, List<T>> map, string key, T value)
    {
        if (key == null)
            return;

        if (!map.TryGetValue(key, out var list))
            map[key] = list = [];

        list.Add(value);
    }

    #endregion

    #region Family blocks

    /// <summary>
    /// Merges every couple into a single wide node. dot's crossing minimisation has
    /// no notion of "keep these two nodes adjacent", so without this a stranger's
    /// card can end up wedged between two spouses.
    /// </summary>
    private Blocks BuildBlocks(IReadOnlyList<TreePersonVM> persons, IReadOnlyList<TreeRelationVM> relations)
    {
        var parent = new Dictionary<string, string>();
        foreach (var p in persons)
            parent[p.Id] = p.Id;

        string Find(string x)
        {
            while (parent[x] != x)
            {
                parent[x] = parent[parent[x]];
                x = parent[x];
            }
            return x;
        }

        foreach (var rel in relations)
        {
            if (!parent.ContainsKey(rel.From) || !parent.ContainsKey(rel.To))
                continue;

            var ra = Find(rel.From);
            var rb = Find(rel.To);
            if (ra != rb)
                parent[ra] = rb;
        }

        var byRoot = new Dictionary<string, Block>();
        var list = new List<Block>();
        var blockOf = new Dictionary<string, string>();

        foreach (var p in persons)
        {
            var root = Find(p.Id);
            if (!byRoot.TryGetValue(root, out var block))
            {
                byRoot[root] = block = new Block { Id = root, Name = "b" + list.Count };
                list.Add(block);
            }

            block.Members.Add(p.Id);
            blockOf[p.Id] = root;
        }

        foreach (var rel in relations)
        {
            if (!parent.ContainsKey(rel.From))
                continue;

            if (byRoot.TryGetValue(Find(rel.From), out var block))
                block.Relations.Add(rel);
        }

        var byName = new Dictionary<string, Block>();
        foreach (var block in list)
        {
            block.Members = ChainMembers(block.Members, block.Relations);
            block.Width = block.Members.Count * _cardWidth + (block.Members.Count - 1) * _coupleGap;
            block.Orders = MemberOrders(block);
            byName[block.Name] = block;
        }

        return new Blocks { List = list, ById = byRoot, ByName = byName, BlockOf = blockOf };
    }

    /// <summary>
    /// Orders a block's members along the marriage chain, so spouses touch.
    /// </summary>
    private static List<string> ChainMembers(List<string> members, List<TreeRelationVM> relations)
    {
        if (members.Count < 2)
            return members;

        var adj = new Dictionary<string, List<string>>();
        foreach (var m in members)
            adj[m] = [];

        foreach (var rel in relations)
        {
            if (adj.ContainsKey(rel.From) && adj.ContainsKey(rel.To))
            {
                adj[rel.From].Add(rel.To);
                adj[rel.To].Add(rel.From);
            }
        }

        var start = members[0];
        foreach (var m in members)
        {
            if (adj[m].Count == 1)
            {
                start = m;
                break;
            }
        }

        var seen = new HashSet<string>();
        var order = new List<string>();
        var stack = new Stack<string>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            var id = stack.Pop();
            if (!seen.Add(id))
                continue;

            order.Add(id);

            var next = adj[id];
            for (var i = next.Count - 1; i >= 0; i--)
            {
                if (!seen.Contains(next[i]))
                    stack.Push(next[i]);
            }
        }

        foreach (var m in members)
        {
            if (!seen.Contains(m))
                order.Add(m);
        }

        return order;
    }

    /// <summary>
    /// The candidate left-to-right arrangements of one block's cards.
    ///
    /// Which spouse stands on which side is free, and picking the wrong side leaves a
    /// long diagonal run to the parents. Small blocks get every permutation that
    /// keeps married pairs touching; larger ones just get the chain and its mirror.
    /// </summary>
    private static List<List<string>> MemberOrders(Block block)
    {
        var members = block.Members;
        if (members.Count < 2)
            return [members];

        if (members.Count > 4)
            return [members, Enumerable.Reverse(members).ToList()];

        var married = new HashSet<string>();
        foreach (var rel in block.Relations)
        {
            married.Add(rel.From + " " + rel.To);
            married.Add(rel.To + " " + rel.From);
        }

        var best = new List<List<string>>();
        var bestPairs = -1;

        Permute(members, order =>
        {
            var pairs = 0;
            for (var j = 1; j < order.Count; j++)
            {
                if (married.Contains(order[j - 1] + " " + order[j]))
                    pairs++;
            }

            if (pairs > bestPairs)
            {
                bestPairs = pairs;
                best = [order];
            }
            else if (pairs == bestPairs)
            {
                best.Add(order);
            }
        });

        return best;
    }

    private static void Permute(List<string> items, Action<List<string>> fn)
    {
        var acc = new List<string>();

        void Step(List<string> rest)
        {
            if (rest.Count == 0)
            {
                fn([..acc]);
                return;
            }

            for (var i = 0; i < rest.Count; i++)
            {
                acc.Add(rest[i]);

                var next = new List<string>(rest.Count - 1);
                next.AddRange(rest.Take(i));
                next.AddRange(rest.Skip(i + 1));
                Step(next);

                acc.RemoveAt(acc.Count - 1);
            }
        }

        Step(items);
    }

    #endregion

    #region Context

    /// <summary>
    /// Precomputes the parent/child wiring the scorer needs, in block terms.
    /// </summary>
    private static Ctx BuildContext(IReadOnlyList<TreePersonVM> persons, IReadOnlyList<TreeRelationVM> relations, Blocks blocks)
    {
        var relById = new Dictionary<string, TreeRelationVM>();
        foreach (var rel in relations)
            relById[rel.Id] = rel;

        // block -> block links, one per distinct pair, as dot sees them
        var links = new List<Link>();
        var seen = new HashSet<string>();

        // union -> children, as the scorer sees them
        var kids = new Dictionary<string, List<string>>();

        foreach (var person in persons)
        {
            if (person.Parents == null)
                continue;

            if (!relById.TryGetValue(person.Parents, out var rel))
                continue;

            Add(kids, rel.Id, person.Id);

            var from = blocks.BlockOf.GetValueOrDefault(rel.From) ?? blocks.BlockOf.GetValueOrDefault(rel.To);
            var to = blocks.BlockOf.GetValueOrDefault(person.Id);
            if (from == null || to == null || from == to)
                continue;

            if (!seen.Add(from + " " + to))     // dot only needs the family link once
                continue;

            links.Add(new Link(blocks.ById[from].Name, blocks.ById[to].Name));
        }

        var ctx = new Ctx { Links = links, Kids = kids, RelById = relById };
        ctx.Kin = BuildKin(relations, ctx, blocks);
        ctx.ParentRelOf = BuildParentRels(ctx);
        return ctx;
    }

    /// <summary>
    /// The pairs of blocks that may stand close together.
    ///
    /// Siblings read as one group and should stay tight; two families with no such
    /// link should be visibly apart. Gathering the children per *person* rather than
    /// per union means half-siblings, who share only one parent, count as kin too.
    /// </summary>
    private static HashSet<string> BuildKin(IReadOnlyList<TreeRelationVM> relations, Ctx ctx, Blocks blocks)
    {
        var byPerson = new Dictionary<string, List<string>>();

        foreach (var rel in relations)
        {
            foreach (var kid in ctx.Kids.GetValueOrDefault(rel.Id) ?? [])
            {
                var root = blocks.BlockOf.GetValueOrDefault(kid);
                if (root == null)
                    continue;

                var blockName = blocks.ById[root].Name;
                Add(byPerson, rel.From, blockName);
                Add(byPerson, rel.To, blockName);
            }
        }

        var kin = new HashSet<string>();
        foreach (var list in byPerson.Values)
        {
            for (var j = 0; j < list.Count; j++)
            {
                for (var k = j + 1; k < list.Count; k++)
                {
                    if (list[j] != list[k])
                        kin.Add(PairKey(list[j], list[k]));
                }
            }
        }

        return kin;
    }

    /// <summary>
    /// Maps every person to the union they descend from.
    /// </summary>
    private static Dictionary<string, TreeRelationVM> BuildParentRels(Ctx ctx)
    {
        var map = new Dictionary<string, TreeRelationVM>();
        foreach (var (relId, list) in ctx.Kids)
        {
            foreach (var personId in list)
                map[personId] = ctx.RelById[relId];
        }
        return map;
    }

    private static string PairKey(string a, string b)
    {
        return string.CompareOrdinal(a, b) < 0 ? a + " " + b : b + " " + a;
    }

    /// <summary>
    /// The minimum gap the row must leave between two neighbouring blocks.
    /// </summary>
    private static double GapBetween(HashSet<string> kin, Block a, Block b)
    {
        return kin.Contains(PairKey(a.Name, b.Name)) ? KinGap : StrangerGap;
    }

    #endregion

    #region DOT source

    private string BuildDot(Blocks blocks, Ctx ctx, IReadOnlyList<int> blockOrder, IReadOnlyList<int> linkOrder)
    {
        var sb = new StringBuilder();
        sb.Append("digraph G {\n");
        sb.Append("  graph [rankdir=TB, ranksep=\"").Append(Num(RankSep)).Append("\", nodesep=\"").Append(Num(NodeSep)).Append("\"];\n");
        sb.Append("  node [shape=box, fixedsize=true, height=\"").Append(Num(_cardHeight / 72)).Append("\", label=\"\"];\n");
        sb.Append("  edge [arrowhead=none];\n");

        foreach (var i in blockOrder)
        {
            var b = blocks.List[i];
            sb.Append("  ").Append(b.Name).Append(" [width=\"").Append(Num(b.Width / 72)).Append("\"];\n");
        }

        foreach (var i in linkOrder)
        {
            var link = ctx.Links[i];
            sb.Append("  ").Append(link.From).Append(" -> ").Append(link.To).Append(";\n");
        }

        sb.Append('}');
        return sb.ToString();
    }

    private static string Num(double value)
    {
        return value.ToString("R", CultureInfo.InvariantCulture);
    }

    #endregion

    #region Search

    /// <summary>
    /// Runs dot over many declaration orders and keeps the tidiest result.
    ///
    /// dot itself has no thoroughness dial - mclimit, nslimit and searchsize all
    /// produce byte-identical output on graphs this shape, because it converges long
    /// before those limits bite. The one thing that does move it is the order the
    /// graph is handed over in, so that is what gets searched.
    ///
    /// The orders come out of a seeded generator and so do not depend on any
    /// previous result, which lets a whole batch of candidates be laid out in one
    /// dot invocation instead of one process per attempt.
    /// </summary>
    private async Task<Dictionary<string, Rect>> SearchAsync(
        IReadOnlyList<TreePersonVM> persons,
        IReadOnlyList<TreeRelationVM> relations,
        Blocks blocks,
        Ctx ctx,
        bool exhaustive,
        CancellationToken token)
    {
        var rnd = new Mulberry32(0x5eed);
        var started = System.Diagnostics.Stopwatch.StartNew();

        Dictionary<string, Rect> best = null;
        var bestCost = double.PositiveInfinity;

        // Nothing to shuffle: a partial tree (dot handles those in one go), a single
        // block, or a forest of blocks dot will just line up side by side whatever
        // order they arrive in.
        var tries = exhaustive && ctx.Links.Count > 0 && blocks.List.Count > 2 ? MaxAttempts : 1;

        var attempt = 0;
        while (attempt < tries)
        {
            // The first batch is kept to the minimum number of attempts, so that a
            // tree slow enough to blow the budget on its own does not first pay for
            // a full batch of candidates it will never look at.
            var batch = new List<string>();
            var batchEnd = Math.Min(attempt + (attempt == 0 ? MinAttempts : BatchSize), tries);
            for (var i = attempt; i < batchEnd; i++)
            {
                var blockOrder = i == 0 ? Identity(blocks.List.Count) : Shuffled(blocks.List.Count, rnd);
                var linkOrder = i == 0 ? Identity(ctx.Links.Count) : Shuffled(ctx.Links.Count, rnd);
                batch.Add(BuildDot(blocks, ctx, blockOrder, linkOrder));
            }

            var layouts = await _graphviz.RenderAsync(batch, token);

            foreach (var layout in layouts)
            {
                var placed = ReadBlocks(layout, blocks);
                if (placed == null)
                    continue;

                var rows = BuildRows(blocks, placed);
                Spread(placed, rows, ctx.Kin);
                PickMemberOrders(blocks, placed, ctx);
                Compact(blocks, placed, ctx, rows);
                PickMemberOrders(blocks, placed, ctx);

                var pos = Expand(blocks, placed, relations);
                var cost = Score(pos, relations, ctx);

                if (best == null || cost < bestCost)
                {
                    bestCost = cost;
                    best = pos;
                }
            }

            attempt = batchEnd;

            if (attempt >= MinAttempts && started.ElapsedMilliseconds > BudgetMs)
                break;
        }

        if (best == null)
            throw new Exception("dot produced no usable layout");

        return best;
    }

    private static int[] Identity(int n)
    {
        var a = new int[n];
        for (var i = 0; i < n; i++)
            a[i] = i;
        return a;
    }

    private static int[] Shuffled(int n, Mulberry32 rnd)
    {
        var a = Identity(n);
        for (var i = n - 1; i > 0; i--)
        {
            var j = (int)Math.Floor(rnd.Next() * (i + 1));
            (a[i], a[j]) = (a[j], a[i]);
        }
        return a;
    }

    /// <summary>
    /// Reads back the block rectangles, in page coordinates.
    /// </summary>
    private Dictionary<string, Placed> ReadBlocks(GraphvizLayout layout, Blocks blocks)
    {
        if (double.IsNaN(layout.Height) || double.IsInfinity(layout.Height))
            return null;

        var placed = new Dictionary<string, Placed>();
        foreach (var (name, point) in layout.Nodes)
        {
            if (!blocks.ByName.TryGetValue(name, out var block))
                continue;

            placed[block.Name] = new Placed
            {
                X = point.X - block.Width / 2,
                Y = layout.Height - point.Y - _cardHeight / 2
            };
        }

        return placed;
    }

    /// <summary>
    /// Groups the placed blocks into rows, each sorted left to right.
    /// </summary>
    private static List<List<Block>> BuildRows(Blocks blocks, Dictionary<string, Placed> placed)
    {
        var rows = new Dictionary<double, List<Block>>();
        foreach (var block in blocks.List)
        {
            if (!placed.TryGetValue(block.Name, out var p))
                continue;

            var key = JsRound(p.Y);
            if (!rows.TryGetValue(key, out var row))
                rows[key] = row = [];

            row.Add(block);
        }

        var rowList = new List<List<Block>>();
        foreach (var key in rows.Keys.OrderBy(x => x))
        {
            var row = rows[key];
            row.Sort((a, b) => placed[a.Name].X.CompareTo(placed[b.Name].X));
            rowList.Add(row);
        }

        return rowList;
    }

    /// <summary>
    /// Pushes families that are not siblings apart.
    ///
    /// dot laid the row out at the tight gap, because nodesep applies to the whole
    /// graph and siblings need it tight. This opens up every other neighbouring pair
    /// to the wide one. Blocks only ever move right, so the row order that dot chose
    /// - and with it the crossing count - survives untouched.
    /// </summary>
    private static void Spread(Dictionary<string, Placed> placed, List<List<Block>> rowList, HashSet<string> kin)
    {
        foreach (var row in rowList)
        {
            for (var j = 1; j < row.Count; j++)
            {
                var prev = row[j - 1];
                var cur = row[j];
                var min = placed[prev.Name].X + prev.Width + GapBetween(kin, prev, cur);
                if (placed[cur.Name].X < min)
                    placed[cur.Name].X = min;
            }
        }
    }

    /// <summary>
    /// Slides blocks into the slack dot left behind.
    ///
    /// dot places a block to straighten the block-to-block links it was given, and it
    /// knows nothing about the union points - the little junctions the marriage and
    /// descent wires actually meet at. So a block often sits several hundred pixels
    /// away from where its wires would like it, with an empty gap in between. This
    /// walks every block towards the median of its own wire endpoints, never past its
    /// neighbours, so the row order, the row width and the crossing count all stay
    /// exactly as dot decided them.
    /// </summary>
    private void Compact(Blocks blocks, Dictionary<string, Placed> placed, Ctx ctx, List<List<Block>> rowList)
    {
        for (var pass = 0; pass < CompactPasses; pass++)
        {
            var moved = false;
            foreach (var row in rowList)
            {
                // Alternating the sweep direction keeps a block from being blocked
                // for good by a neighbour that has not been pulled along yet.
                var order = Identity(row.Count);
                if (pass % 2 == 1)
                    Array.Reverse(order);

                foreach (var idx in order)
                {
                    var b = row[idx];
                    var target = DesiredX(blocks, placed, ctx, b);
                    if (target == null)
                        continue;

                    var low = idx > 0
                        ? placed[row[idx - 1].Name].X + row[idx - 1].Width + GapBetween(ctx.Kin, row[idx - 1], b)
                        : double.NegativeInfinity;
                    var high = idx < row.Count - 1
                        ? placed[row[idx + 1].Name].X - b.Width - GapBetween(ctx.Kin, b, row[idx + 1])
                        : double.PositiveInfinity;

                    var x = Math.Min(Math.Max(target.Value, low), high);
                    if (Math.Abs(x - placed[b.Name].X) > 0.5)
                    {
                        placed[b.Name].X = x;
                        moved = true;
                    }
                }
            }

            if (!moved)
                break;
        }
    }

    /// <summary>
    /// The x this block's wires would put it at: the median of one target per wire,
    /// which is the point that minimises their total horizontal run.
    /// </summary>
    private double? DesiredX(Blocks blocks, Dictionary<string, Placed> placed, Ctx ctx, Block block)
    {
        var order = block.Order ?? block.Members;
        var targets = new List<double>();

        // wires up to each member's own parents
        for (var i = 0; i < order.Count; i++)
        {
            var rel = ctx.ParentRelOf.GetValueOrDefault(order[i]);
            if (rel == null)
                continue;

            var u = UnionPoint(blocks, placed, rel);
            if (u != null)
                targets.Add(u.Value.X - i * (_cardWidth + _coupleGap) - _cardWidth / 2);
        }

        // wires down from each of the block's own unions to their children
        foreach (var r in block.Relations)
        {
            var ia = order.IndexOf(r.From);
            var ib = order.IndexOf(r.To);
            if (ia < 0 && ib < 0)
                continue;

            var offset = ia >= 0 && ib >= 0
                ? (ia + ib) / 2.0 * (_cardWidth + _coupleGap) + _cardWidth / 2
                : Math.Max(ia, ib) * (_cardWidth + _coupleGap) + _cardWidth / 2;

            foreach (var kid in ctx.Kids.GetValueOrDefault(r.Id) ?? [])
            {
                var c = CardCentre(blocks, placed, kid);
                if (c != null)
                    targets.Add(c.Value.X - offset);
            }
        }

        if (targets.Count == 0)
            return null;

        targets.Sort();
        var mid = targets.Count >> 1;
        return targets.Count % 2 == 1
            ? targets[mid]
            : (targets[mid - 1] + targets[mid]) / 2;
    }

    /// <summary>
    /// Chooses, for every block, which of its candidate arrangements puts the fewest
    /// long horizontal runs on the wires around it.
    /// </summary>
    private void PickMemberOrders(Blocks blocks, Dictionary<string, Placed> placed, Ctx ctx)
    {
        foreach (var block in blocks.List)
            block.Order = block.Orders[0];

        // Two sweeps are enough: the first fixes each block against its neighbours'
        // default arrangement, the second against their chosen one.
        for (var pass = 0; pass < 2; pass++)
        {
            foreach (var b in blocks.List)
            {
                if (b.Orders.Count < 2 || !placed.ContainsKey(b.Name))
                    continue;

                var bestOrder = b.Order;
                var bestCost = double.PositiveInfinity;

                foreach (var candidate in b.Orders)
                {
                    b.Order = candidate;
                    var c = LocalCost(blocks, placed, ctx, b);
                    if (c < bestCost)
                    {
                        bestCost = c;
                        bestOrder = candidate;
                    }
                }

                b.Order = bestOrder;
            }
        }
    }

    /// <summary>
    /// Where a person's card lands, given the current arrangement of its block.
    /// </summary>
    private Point? CardCentre(Blocks blocks, Dictionary<string, Placed> placed, string personId)
    {
        var root = blocks.BlockOf.GetValueOrDefault(personId);
        if (root == null || !blocks.ById.TryGetValue(root, out var block))
            return null;

        if (!placed.TryGetValue(block.Name, out var p))
            return null;

        var order = block.Order ?? block.Members;
        var idx = order.IndexOf(personId);
        if (idx < 0)
            return null;

        return new Point(p.X + idx * (_cardWidth + _coupleGap) + _cardWidth / 2, p.Y);
    }

    /// <summary>
    /// Horizontal run of every wire touching the given block.
    /// </summary>
    private double LocalCost(Blocks blocks, Dictionary<string, Placed> placed, Ctx ctx, Block block)
    {
        var total = 0d;
        foreach (var rel in block.Relations)
            total += UnionCost(blocks, placed, ctx, rel);

        // marriages of the block's members that live in some other block cannot
        // happen - a marriage always merges both spouses into one block - so the
        // only other wires are the ones down to this block's members' own parents.
        var order = block.Order ?? block.Members;
        foreach (var member in order)
        {
            var rel = ctx.ParentRelOf.GetValueOrDefault(member);
            if (rel == null)
                continue;

            var u = UnionPoint(blocks, placed, rel);
            var c = CardCentre(blocks, placed, member);
            if (u != null && c != null)
                total += Math.Abs(u.Value.X - c.Value.X);
        }

        return total;
    }

    private Point? UnionPoint(Blocks blocks, Dictionary<string, Placed> placed, TreeRelationVM rel)
    {
        var a = CardCentre(blocks, placed, rel.From);
        var b = CardCentre(blocks, placed, rel.To);
        var one = a ?? b;
        if (one == null)
            return null;

        var x = a != null && b != null ? (a.Value.X + b.Value.X) / 2 : one.Value.X;
        var y = (a != null && b != null ? Math.Max(a.Value.Y, b.Value.Y) : one.Value.Y) + _cardHeight + UnionDrop;
        return new Point(x, y);
    }

    /// <summary>
    /// Horizontal run of the marriage wires and the descent wires of one union.
    /// </summary>
    private double UnionCost(Blocks blocks, Dictionary<string, Placed> placed, Ctx ctx, TreeRelationVM rel)
    {
        var u = UnionPoint(blocks, placed, rel);
        if (u == null)
            return 0;

        var total = 0d;
        var a = CardCentre(blocks, placed, rel.From);
        var b = CardCentre(blocks, placed, rel.To);
        if (a != null)
            total += Math.Abs(a.Value.X - u.Value.X);
        if (b != null)
            total += Math.Abs(b.Value.X - u.Value.X);

        foreach (var kid in ctx.Kids.GetValueOrDefault(rel.Id) ?? [])
        {
            var c = CardCentre(blocks, placed, kid);
            if (c != null)
                total += Math.Abs(c.Value.X - u.Value.X);
        }

        return total;
    }

    /// <summary>
    /// Turns block rectangles plus chosen arrangements into per-person boxes.
    /// </summary>
    private Dictionary<string, Rect> Expand(Blocks blocks, Dictionary<string, Placed> placed, IReadOnlyList<TreeRelationVM> relations)
    {
        var pos = new Dictionary<string, Rect>();

        foreach (var block in blocks.List)
        {
            if (!placed.TryGetValue(block.Name, out var p))
                continue;

            var order = block.Order ?? block.Members;
            var x = p.X;
            foreach (var member in order)
            {
                pos[member] = new Rect { X = x, Y = p.Y, Width = _cardWidth, Height = _cardHeight };
                x += _cardWidth + _coupleGap;
            }
        }

        // The union point sits centred between the two spouses, just below their row.
        foreach (var rel in relations)
        {
            var a = pos.GetValueOrDefault(rel.From);
            var b = pos.GetValueOrDefault(rel.To);
            var one = a ?? b;
            if (one == null)
                continue;

            var cxu = a != null && b != null
                ? (a.X + a.Width / 2 + b.X + b.Width / 2) / 2
                : one.X + one.Width / 2;
            var cyu = (a != null && b != null ? Math.Max(a.Y, b.Y) : one.Y) + _cardHeight + UnionDrop;

            pos[rel.Id] = new Rect { X = cxu - 0.5, Y = cyu - 0.5, Width = 1, Height = 1 };
        }

        return pos;
    }

    /// <summary>
    /// How tangled a candidate layout is: horizontal wire run, plus its width.
    /// </summary>
    private static double Score(Dictionary<string, Rect> pos, IReadOnlyList<TreeRelationVM> relations, Ctx ctx)
    {
        var total = 0d;

        foreach (var rel in relations)
        {
            var u = pos.GetValueOrDefault(rel.Id);
            if (u == null)
                continue;

            var ux = u.X + 0.5;

            var a = pos.GetValueOrDefault(rel.From);
            var b = pos.GetValueOrDefault(rel.To);
            if (a != null)
                total += Math.Abs(a.X + a.Width / 2 - ux);
            if (b != null)
                total += Math.Abs(b.X + b.Width / 2 - ux);

            foreach (var kid in ctx.Kids.GetValueOrDefault(rel.Id) ?? [])
            {
                var c = pos.GetValueOrDefault(kid);
                if (c == null)
                    continue;

                total += Math.Abs(c.X + c.Width / 2 - ux) + Math.Abs(c.Y - u.Y);
            }
        }

        var minX = double.PositiveInfinity;
        var maxX = double.NegativeInfinity;
        foreach (var p in pos.Values)
        {
            if (p.X < minX)
                minX = p.X;
            if (p.X + p.Width > maxX)
                maxX = p.X + p.Width;
        }

        return total + WidthWeight * (maxX - minX);
    }

    #endregion

    #region Finishing touches

    /// <summary>
    /// Slides each union point sideways to line up with its children.
    ///
    /// The point starts centred between the two spouses, so unless the children
    /// happen to be centred there too the descent wire leaves with a short sideways
    /// jog - and a jog of a dozen pixels reads as a kink rather than as a step.
    ///
    /// The point is free to sit anywhere between the two cards' centres: it lives
    /// below both cards, so even at the extremes it only turns one marriage wire
    /// into a straight drop and lengthens the other. That trade is worth it - on the
    /// reference tree this both removes every sub-40px kink and shortens the total
    /// wire run. Runs after the search, on the winning layout only; the search itself
    /// keeps the simpler centred model, which this can only improve on.
    /// </summary>
    private void AlignUnions(Dictionary<string, Rect> pos, IReadOnlyList<TreeRelationVM> relations, Ctx ctx)
    {
        foreach (var rel in relations)
        {
            var u = pos.GetValueOrDefault(rel.Id);
            var a = pos.GetValueOrDefault(rel.From);
            var b = pos.GetValueOrDefault(rel.To);
            if (u == null || a == null || b == null)
                continue;

            var sum = 0d;
            var n = 0;
            foreach (var kid in ctx.Kids.GetValueOrDefault(rel.Id) ?? [])
            {
                var c = pos.GetValueOrDefault(kid);
                if (c == null)
                    continue;

                sum += c.X + c.Width / 2;
                n++;
            }

            if (n == 0)
                continue;

            var lo = Math.Min(a.X, b.X) + _cardWidth / 2;   // centre of the left card
            var hi = Math.Max(a.X, b.X) + _cardWidth / 2;   // centre of the right card
            if (hi < lo)
                continue;                                   // spouses not side by side

            u.X = Math.Min(Math.Max(sum / n, lo), hi) - u.Width / 2;
        }
    }

    private static void Normalize(Dictionary<string, Rect> pos)
    {
        var minX = double.PositiveInfinity;
        var minY = double.PositiveInfinity;

        foreach (var p in pos.Values)
        {
            if (p.X < minX)
                minX = p.X;
            if (p.Y < minY)
                minY = p.Y;
        }

        if (double.IsInfinity(minX))
            return;

        foreach (var p in pos.Values)
        {
            p.X -= minX - Spacing;
            p.Y -= minY - Spacing;
        }
    }

    private static (long Width, long Height) Measure(Dictionary<string, Rect> pos)
    {
        var maxX = 0d;
        var maxY = 0d;

        foreach (var p in pos.Values)
        {
            if (p.X + p.Width > maxX)
                maxX = p.X + p.Width;
            if (p.Y + p.Height > maxY)
                maxY = p.Y + p.Height;
        }

        return ((long)Math.Ceiling(maxX + Spacing), (long)Math.Ceiling(maxY + Spacing));
    }

    private static JArray BuildChildren(IReadOnlyList<TreePersonVM> persons, IReadOnlyList<TreeRelationVM> relations, Dictionary<string, Rect> pos)
    {
        var children = new JArray();

        foreach (var person in persons)
        {
            var p = pos.GetValueOrDefault(person.Id);
            if (p == null)
                continue;

            children.Add(new JObject
            {
                ["id"] = person.Id,
                ["label"] = person.Name,
                ["x"] = p.X,
                ["y"] = p.Y,
                ["width"] = p.Width,
                ["height"] = p.Height,
                ["info"] = JObject.FromObject(person)
            });
        }

        // Union nodes carry no `info`, which is how the front-end tells them apart
        // from cards (see convertPersons / detectChildren in tree.js).
        foreach (var rel in relations)
        {
            var u = pos.GetValueOrDefault(rel.Id);
            if (u == null)
                continue;

            children.Add(new JObject
            {
                ["id"] = rel.Id,
                ["x"] = u.X,
                ["y"] = u.Y,
                ["width"] = 1,
                ["height"] = 1
            });
        }

        return children;
    }

    /// <summary>
    /// Mirrors the finished layout around the horizontal axis of the canvas.
    ///
    /// The whole layout is built top-down - dot ranks parents above children, the
    /// marriage brackets hang below the cards, the sibling buses run above them - so
    /// bottom-to-top is a mirror of the finished geometry rather than a second layout
    /// mode. Every wire is orthogonal, so mirroring leaves the corners square, and
    /// the cards themselves are only moved, never turned over.
    /// </summary>
    private static void FlipVertical(JObject tree, double height)
    {
        foreach (var child in (JArray)tree["children"])
            child["y"] = height - (child["y"].Value<double>() + child["height"].Value<double>());

        foreach (var edge in (JArray)tree["edges"])
        {
            var section = (JObject)((JArray)edge["sections"])[0];
            section["startPoint"]["y"] = height - section["startPoint"]["y"].Value<double>();
            section["endPoint"]["y"] = height - section["endPoint"]["y"].Value<double>();

            foreach (var bend in (JArray)section["bendPoints"])
                bend["y"] = height - bend["y"].Value<double>();
        }
    }

    #endregion

    #region Routing

    /// <summary>
    /// Groups cards into per-row bands, so trunks can be steered around them.
    /// </summary>
    private static List<Band> BuildBands(Dictionary<string, Rect> pos, IReadOnlyList<TreePersonVM> persons)
    {
        var rows = new Dictionary<double, Band>();

        foreach (var person in persons)
        {
            var p = pos.GetValueOrDefault(person.Id);
            if (p == null)
                continue;

            var key = JsRound(p.Y);
            if (!rows.TryGetValue(key, out var band))
                rows[key] = band = new Band { Y1 = p.Y, Y2 = p.Y + p.Height };

            if (p.Y + p.Height > band.Y2)
                band.Y2 = p.Y + p.Height;

            band.Spans.Add(new Span(person.Id, p.X, p.X + p.Width));
        }

        var bands = rows.Values.OrderBy(x => x.Y1).ToList();
        for (var i = 0; i < bands.Count; i++)
        {
            bands[i].Spans = bands[i].Spans.OrderBy(x => x.X1).ToList();
            bands[i].Index = i;
        }

        return bands;
    }

    /// <summary>
    /// Index of the row a card row's y belongs to, which is also the number of the
    /// band directly above it.
    /// </summary>
    private static int RowIndex(List<Band> bands, double rowY)
    {
        for (var i = 0; i < bands.Count; i++)
        {
            if (Math.Abs(bands[i].Y1 - rowY) < 0.5)
                return i;
        }

        return 0;
    }

    private static bool IsFree(Band band, double x, string exceptId)
    {
        foreach (var s in band.Spans)
        {
            if (s.Id == exceptId)
                continue;

            if (x > s.X1 && x < s.X2)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Nearest x to wantX that clears every card in the band.
    /// </summary>
    private static double FreeX(Band band, double wantX, string exceptId)
    {
        var spans = band.Spans.Where(x => x.Id != exceptId).ToList();
        if (spans.Count == 0 || IsFree(band, wantX, exceptId))
            return wantX;

        var gaps = new List<(double From, double To)>
        {
            (double.NegativeInfinity, spans[0].X1 - CardClearance)
        };

        for (var i = 1; i < spans.Count; i++)
        {
            var from = spans[i - 1].X2 + CardClearance;
            var to = spans[i].X1 - CardClearance;
            if (to > from)
                gaps.Add((from, to));
        }

        gaps.Add((spans[^1].X2 + CardClearance, double.PositiveInfinity));

        var best = wantX;
        var bestDist = double.PositiveInfinity;
        foreach (var gap in gaps)
        {
            var c = Math.Min(Math.Max(wantX, gap.From), gap.To);
            var d = Math.Abs(c - wantX);
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        return best;
    }

    /// <summary>
    /// Walks a trunk down from (x, yFrom) to yTo, stepping sideways into a gap
    /// whenever a row of cards is in the way: a line may cross another line, but
    /// never a card.
    /// </summary>
    private static List<Point> Descend(List<Band> bands, LaneMap lanes, double x, double yFrom, double yTo, double wantX, string exceptId)
    {
        var pts = new List<Point> { new(x, yFrom) };
        var cur = x;

        foreach (var band in bands)
        {
            if (band.Y2 <= yFrom + 0.5 || band.Y1 >= yTo - 0.5)
                continue;

            if (IsFree(band, cur, exceptId))
                continue;

            // The dodge runs just above the row it is getting past, which is the
            // same shelf the sibling buses of that row live on - so it queues for a
            // lane there like they do.
            var lane = FreeX(band, wantX, exceptId);
            var laneY = band.Y1 - lanes.Offset(lanes.Take(band.Index, true, cur, lane));
            pts.Add(new Point(cur, laneY));
            pts.Add(new Point(lane, laneY));
            cur = lane;
        }

        pts.Add(new Point(cur, yTo));
        return pts;
    }

    private JArray RouteEdges(IReadOnlyList<TreePersonVM> persons, IReadOnlyList<TreeRelationVM> relations, Dictionary<string, Rect> pos, List<Band> bands, LaneMap lanes)
    {
        var edges = new JArray();

        // --- marriage lines: person -> union ---------------------------------
        // Someone with several marriages gets one exit port per marriage, spread
        // across the bottom of the card and ordered by which side the union is on.
        var bySpouse = new Dictionary<string, List<TreeRelationVM>>();
        var spouseOrder = new List<string>();

        void PushSpouse(string personId, TreeRelationVM rel)
        {
            if (!pos.ContainsKey(personId) || !pos.ContainsKey(rel.Id))
                return;

            if (!bySpouse.TryGetValue(personId, out var list))
            {
                bySpouse[personId] = list = [];
                spouseOrder.Add(personId);
            }

            list.Add(rel);
        }

        foreach (var rel in relations)
        {
            PushSpouse(rel.From, rel);
            PushSpouse(rel.To, rel);
        }

        var ports = new Dictionary<(string Person, string Rel), double>();

        foreach (var personId in spouseOrder)
        {
            var card = pos[personId];
            var rels = bySpouse[personId].OrderBy(x => pos[x.Id].X).ToList();
            bySpouse[personId] = rels;

            var usable = card.Width - _portInset * 2;
            for (var j = 0; j < rels.Count; j++)
            {
                ports[(personId, rels[j].Id)] = rels.Count == 1
                    ? card.X + card.Width / 2
                    : card.X + _portInset + usable * (j + 1) / (rels.Count + 1);
            }
        }

        // The union point hangs in the band under the lower of the two spouses, and
        // the bracket that reaches it is a horizontal run like any other - so the
        // bracket queues for a lane in that band and the point settles at whatever
        // height the lane it got sits at. Two brackets that used to merge into one
        // line (a marriage reaching across an intervening card, or one drawn down
        // from the row above) now sit a lane apart.
        foreach (var rel in relations)
        {
            var u = pos.GetValueOrDefault(rel.Id);
            var a = pos.GetValueOrDefault(rel.From);
            var b = pos.GetValueOrDefault(rel.To);
            var one = a ?? b;
            if (u == null || one == null)
                continue;

            var rowY = a != null && b != null ? Math.Max(a.Y, b.Y) : one.Y;
            var ux = u.X + u.Width / 2;
            var from = ux;
            var to = ux;

            if (a != null && ports.TryGetValue((rel.From, rel.Id), out var pa))
                (from, to) = (Math.Min(from, pa), Math.Max(to, pa));

            if (b != null && ports.TryGetValue((rel.To, rel.Id), out var pb))
                (from, to) = (Math.Min(from, pb), Math.Max(to, pb));

            var lane = lanes.Take(RowIndex(bands, rowY) + 1, false, from, to);
            u.Y = rowY + _cardHeight + lanes.Offset(lane) - u.Height / 2;
        }

        foreach (var personId in spouseOrder)
        {
            var card = pos[personId];

            foreach (var rel in bySpouse[personId])
            {
                var px = ports[(personId, rel.Id)];
                var u = pos[rel.Id];
                var ux = u.X + u.Width / 2;
                var uy = u.Y + u.Height / 2;

                List<Point> pts;
                if (uy >= card.Y + card.Height)
                {
                    pts = Descend(bands, lanes, px, card.Y + card.Height, uy, ux, personId);
                    pts.Add(new Point(ux, uy));
                }
                else
                {
                    pts = [new Point(px, card.Y), new Point(px, uy), new Point(ux, uy)];
                }

                // The wire stops exactly on the union point, so the front-end must
                // not trim its last segment - doing so would leave a childless
                // marriage as two stubs that never meet.
                edges.Add(MakeEdge(rel.Id + ":" + personId, personId, rel.Id + ":n", pts));
            }
        }

        // --- descent lines: union -> children --------------------------------
        var byUnion = new Dictionary<string, List<TreePersonVM>>();
        var unionOrder = new List<string>();

        foreach (var person in persons)
        {
            if (person.Parents == null || !pos.ContainsKey(person.Parents) || !pos.ContainsKey(person.Id))
                continue;

            if (!byUnion.TryGetValue(person.Parents, out var list))
            {
                byUnion[person.Parents] = list = [];
                unionOrder.Add(person.Parents);
            }

            list.Add(person);
        }

        foreach (var unionId in unionOrder)
        {
            var un = pos[unionId];
            var unx = un.X + un.Width / 2;
            var uny = un.Y + un.Height / 2;

            // Children that ended up on different rows each get their own bus; a
            // single shared bus would leave the lower ones hanging off a drop that
            // has to cross whatever sits in between.
            var rowsOf = new Dictionary<double, List<TreePersonVM>>();
            foreach (var kid in byUnion[unionId])
            {
                var rowKey = JsRound(pos[kid.Id].Y);
                if (!rowsOf.TryGetValue(rowKey, out var list))
                    rowsOf[rowKey] = list = [];

                list.Add(kid);
            }

            foreach (var rowKey in rowsOf.Keys.OrderBy(x => x))
            {
                var group = rowsOf[rowKey];
                var rowY = pos[group[0].Id].Y;

                var centre = group.Sum(x => pos[x.Id].X + _cardWidth / 2) / group.Count;
                var leftmost = group.Min(x => pos[x.Id].X + _cardWidth / 2);
                var rightmost = group.Max(x => pos[x.Id].X + _cardWidth / 2);

                // The trunk is walked down to the base lane first: which rows it has
                // to get past does not depend on the lane the bus ends up in, and
                // where the trunk lands is one end of the run the bus has to cover.
                var trunk = Descend(bands, lanes, unx, uny, Math.Max(rowY - LaneStep, uny), centre, null);
                var landing = trunk[^1].X;

                // The sideways step of a lone child lands halfway down the band, one
                // LaneStep below the union and one above the card, so it reads as a
                // deliberate step rather than a notch against the card. Where two
                // buses of the same row cover the same stretch of x - two unrelated
                // couples whose descents reach across one another - they go on
                // different shelves, so the pair never reads as one sibling bus.
                var lane = lanes.Take(RowIndex(bands, rowY), true, Math.Min(landing, leftmost), Math.Max(landing, rightmost));
                var busY = Math.Max(rowY - lanes.Offset(lane), uny);
                trunk[^1] = new Point(landing, busY);

                foreach (var child in group)
                {
                    var cp = pos[child.Id];
                    var ccx = cp.X + cp.Width / 2;

                    var pts = new List<Point>(trunk) { new(ccx, busY), new(ccx, cp.Y) };
                    edges.Add(MakeEdge(child.Id + ":" + unionId, unionId + ":s", child.Id + ":n", pts));
                }
            }
        }

        return edges;
    }

    /// <summary>
    /// Pushes the rows apart until every band is deep enough for the lanes the probe
    /// pass asked for.
    ///
    /// A band needs one LaneStep of clearance below the cards above it, one for each
    /// marriage bracket lane, one for each sibling bus lane, and one between the two
    /// families of lanes. For a band holding a single bracket lane and a single bus
    /// lane that is exactly the RowGap dot already left, so a tree with no overlapping
    /// runs does not grow at all. Every row below a band that has to open up moves
    /// down with it; the union points are left alone, the routing pass recomputes
    /// them from the cards.
    /// </summary>
    private void ExpandRows(Dictionary<string, Rect> pos, IReadOnlyList<TreePersonVM> persons, List<Band> bands, LaneMap lanes)
    {
        var shift = new Dictionary<double, double>();
        var total = 0d;

        for (var i = 0; i < bands.Count; i++)
        {
            if (i > 0)
            {
                var gap = bands[i].Y1 - bands[i - 1].Y2;
                var need = (lanes.Count(i, false) + lanes.Count(i, true) + 1) * LaneStep;
                if (need > gap)
                    total += need - gap;
            }

            shift[JsRound(bands[i].Y1)] = total;
        }

        if (total == 0)
            return;

        foreach (var person in persons)
        {
            var p = pos.GetValueOrDefault(person.Id);
            if (p != null && shift.TryGetValue(JsRound(p.Y), out var delta))
                p.Y += delta;
        }
    }

    /// <summary>
    /// Packs a polyline into the section shape the front-end expects.
    /// </summary>
    private static JObject MakeEdge(string id, string source, string target, List<Point> points)
    {
        var clean = new List<Point>();
        foreach (var point in points)
        {
            if (clean.Count == 0 || clean[^1] != point)
                clean.Add(point);
        }

        if (clean.Count < 2)
            clean.Add(clean[0]);

        // Drop points that sit on the straight line between their neighbours: the
        // trunk builder emits one per row it walks past, and a redundant bend makes
        // a straight run look like a decision.
        for (var k = clean.Count - 2; k > 0; k--)
        {
            var prev = clean[k - 1];
            var cur = clean[k];
            var next = clean[k + 1];

            if ((prev.X == cur.X && cur.X == next.X) || (prev.Y == cur.Y && cur.Y == next.Y))
                clean.RemoveAt(k);
        }

        var bends = new JArray();
        for (var j = 1; j < clean.Count - 1; j++)
            bends.Add(new JObject { ["x"] = clean[j].X, ["y"] = clean[j].Y });

        return new JObject
        {
            ["id"] = id,
            ["sources"] = new JArray(source),
            ["targets"] = new JArray(target),
            ["info"] = new JObject { ["fakeTarget"] = false },
            ["sections"] = new JArray(new JObject
            {
                ["id"] = id + "_s0",
                ["startPoint"] = new JObject { ["x"] = clean[0].X, ["y"] = clean[0].Y },
                ["endPoint"] = new JObject { ["x"] = clean[^1].X, ["y"] = clean[^1].Y },
                ["bendPoints"] = bends
            })
        };
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Rounds the way JavaScript's Math.round does - halves go up, not to even -
    /// because the row keys and the gap constants are derived from it.
    /// </summary>
    private static double JsRound(double value)
    {
        return Math.Floor(value + 0.5);
    }

    /// <summary>
    /// The mulberry32 generator, so the shuffled declaration orders are the same
    /// on every run and on every machine.
    /// </summary>
    private sealed class Mulberry32
    {
        public Mulberry32(uint seed)
        {
            _state = seed;
        }

        private uint _state;

        public double Next()
        {
            unchecked
            {
                _state += 0x6D2B79F5;
                var a = _state;

                // Math.imul(a ^ (a >>> 15), 1 | a): the low 32 bits of the product.
                var t = (uint)((int)(a ^ (a >> 15)) * (int)(1 | a));

                var inner = (uint)((int)(t ^ (t >> 7)) * (int)(61 | t));
                t = (t + inner) ^ t;

                return (t ^ (t >> 14)) / 4294967296d;
            }
        }
    }

    #endregion

    #region Types

    private class Block
    {
        public string Id { get; init; }
        public string Name { get; init; }
        public List<string> Members { get; set; } = [];
        public List<TreeRelationVM> Relations { get; } = [];
        public double Width { get; set; }
        public List<List<string>> Orders { get; set; }
        public List<string> Order { get; set; }
    }

    private class Blocks
    {
        public List<Block> List { get; init; }
        public Dictionary<string, Block> ById { get; init; }
        public Dictionary<string, Block> ByName { get; init; }
        public Dictionary<string, string> BlockOf { get; init; }
    }

    private class Ctx
    {
        public List<Link> Links { get; init; }
        public Dictionary<string, List<string>> Kids { get; init; }
        public Dictionary<string, TreeRelationVM> RelById { get; init; }
        public HashSet<string> Kin { get; set; }
        public Dictionary<string, TreeRelationVM> ParentRelOf { get; set; }
    }

    private readonly record struct Link(string From, string To);

    private readonly record struct Point(double X, double Y);

    private class Placed
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    private class Rect
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; init; }
        public double Height { get; init; }
    }

    private class Band
    {
        public int Index { get; set; }
        public double Y1 { get; init; }
        public double Y2 { get; set; }
        public List<Span> Spans { get; set; } = [];
    }

    /// <summary>
    /// Keeps two horizontal runs from landing on top of one another.
    ///
    /// Every horizontal run of every wire lives in one of the bands between two rows
    /// of cards, and a band carries two families of them: the marriage brackets just
    /// under the parents, and the sibling buses just over the children (a trunk that
    /// steps sideways to get past a row uses that row's bus shelf). A family used to
    /// be a single y, so two runs whose x ranges overlapped came out as one line -
    /// and one line under two adjacent cards reads as a sibling bus, which is how a
    /// husband and wife end up looking like a brother and sister. This hands each run
    /// a lane number instead: the lowest one whose runs all clear it sideways.
    /// </summary>
    private sealed class LaneMap
    {
        private const double Eps = 1;   // runs that merely touch are not a problem

        private readonly Dictionary<(int Band, bool Bus), List<List<(double From, double To)>>> _groups = new();

        /// <summary>
        /// Probe pass: still count the lanes, but report every one of them at the
        /// base offset, so the lanes are measured on the geometry dot produced rather
        /// than on a half-expanded version of it.
        /// </summary>
        public bool Flat { get; init; }

        /// <summary>
        /// Claims a lane for a run covering [x1, x2] and returns its lane number.
        /// </summary>
        public int Take(int band, bool bus, double x1, double x2)
        {
            var from = Math.Min(x1, x2);
            var to = Math.Max(x1, x2);

            // Nothing is drawn, so nothing can be mistaken for a bus, and the lane it
            // nominally sits in stays free for a run that is actually visible.
            if (to - from <= Eps)
                return 0;

            if (!_groups.TryGetValue((band, bus), out var lanes))
                _groups[(band, bus)] = lanes = [];

            for (var i = 0; i < lanes.Count; i++)
            {
                if (lanes[i].Any(x => from < x.To - Eps && to > x.From + Eps))
                    continue;

                lanes[i].Add((from, to));
                return i;
            }

            lanes.Add([(from, to)]);
            return lanes.Count - 1;
        }

        /// <summary>
        /// How many lanes one family of runs in one band ended up needing.
        /// </summary>
        public int Count(int band, bool bus) => _groups.GetValueOrDefault((band, bus))?.Count ?? 0;

        /// <summary>
        /// How far from the edge of the band a lane hangs.
        /// </summary>
        public double Offset(int lane) => Flat ? LaneStep : LaneStep * (1 + lane);
    }

    private readonly record struct Span(string Id, double X1, double X2);

    #endregion
}
