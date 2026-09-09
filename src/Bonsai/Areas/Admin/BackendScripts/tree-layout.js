var Viz = require('./viz.cjs');

var SPACING = 30,         // margin around the whole canvas
    RANKSEP = 0.75,       // dot units (inches); 0.42 starts producing crossings
    NODESEP = 0.56;

// The card geometry of the two views. Everything about a row is measured in
// these numbers, so switching the view is a matter of setting them before the
// layout runs: dot gets smaller nodes and every gap expressed in card terms
// follows along. The compact card is half as wide, a little shorter, and keeps
// its couple gap and marriage ports in proportion to that.
var VIEWS = {
    Default: { cardWidth: 300, cardHeight: 100, coupleGap: 60, portInset: 40 },
    Compact: { cardWidth: 150, cardHeight: 76, coupleGap: 30, portInset: 20 }
};

var CARD_WIDTH,           // card box, as the front-end will render it
    CARD_HEIGHT,
    COUPLE_GAP,           // gap between two spouses inside one family block
    PORT_INSET;           // keeps the marriage ports off the card corners

applyView('Default');

// The vertical band between two rows of cards, and how the wires divide it up.
// All three horizontal runs that live in the band - the marriage brackets, the
// sibling buses, and the lanes a trunk steps into to get past a row - need room
// of their own, otherwise they collapse into one another and read as noise.
var ROW_GAP = Math.round(RANKSEP * 72),
    UNION_DROP = Math.round(ROW_GAP / 3),   // card bottom -> union point
    BUS_RISE = Math.round(ROW_GAP / 3),     // sibling bus -> child row top
    CARD_CLEARANCE = 20;                    // sideways room when dodging a card

// A row wants two different minimum gaps, and dot's nodesep is a single number
// for the whole graph. So dot lays the row out at the tight one and spread()
// opens up the pairs that deserve the wide one.
var KIN_GAP = Math.round(NODESEP * 72),     // between siblings: keep the group tight
    STRANGER_GAP = KIN_GAP * 2;             // between families with no sibling link

// dot's result depends on the order the nodes and edges are declared in, and the
// spread between a lucky and an unlucky order is large (25% on total edge length
// on the reference tree). So the layout is run many times over shuffled orders
// and the best-scoring one is kept.
// This only pays off for the full tree: the partial trees (ancestors, descendants,
// close family) are narrow and near-hierarchical, dot gets them right on the first
// order, and they are rendered once per page - so they run a single attempt.
var MIN_ATTEMPTS = 8,       // always try at least this many orders
    MAX_ATTEMPTS = 400,
    BUDGET_MS = 2000,      // stop shuffling once this much time has been spent
    COMPACT_PASSES = 24,    // sweeps of the slide-into-the-slack pass
    WIDTH_WEIGHT = 8;     // px of edge length one px of extra width is worth

var vizPromise = null;

/**
 * Calculates the layout of a tree.
 * The second argument used to be the ELK thoroughness; it now tells the layout
 * whether to search over many declaration orders (full tree) or to take dot's
 * first result as it comes (partial trees).
 * The third one carries the display settings: { direction, view }, matching the
 * TreeDirection and TreeViewMode names on the backend. Both are optional and
 * fall back to a top-to-bottom tree of default-sized cards.
 */
module.exports = function (callback, jsonIn, exhaustive, options) {
    try {
        if (vizPromise === null)
            vizPromise = Viz.instance();

        vizPromise.then(function (viz) {
            try {
                var data = JSON.parse(jsonIn);
                callback(null, JSON.stringify(layout(viz, data, exhaustive !== false, options)));
            } catch (err) {
                callback(err, null);
            }
        }, function (err) {
            vizPromise = null;      // allow a retry on the next invocation
            callback(err, null);
        });
    } catch (err) {
        callback(err, null);
    }
};

// ---------------------------------------------------------------- layout

function layout(viz, data, exhaustive, options) {
    var opts = options || {};
    applyView(opts.view);

    var persons = canonicalPersons(data.Persons || [], data.Relations || []);
    var relations = (data.Relations || []).slice().sort(function (a, b) {
        return a.Id < b.Id ? -1 : a.Id > b.Id ? 1 : 0;
    });

    var blocks = buildBlocks(persons, relations);
    var ctx = buildContext(persons, relations, blocks);

    var best = search(viz, persons, relations, blocks, ctx, exhaustive);
    var pos = best.pos;

    normalize(pos);
    alignUnions(pos, relations, ctx);

    var bands = buildBands(pos, persons);
    var edges = routeEdges(persons, relations, pos, bands);
    var size = measure(pos);

    var tree = {
        id: 'root',
        width: size.width,
        height: size.height,
        children: buildChildren(persons, relations, pos),
        edges: edges
    };

    if (isBottomToTop(opts.direction))
        flipVertical(tree);

    return tree;
}

/** Sets the card geometry for the requested view. Unknown names get the default. */
function applyView(view) {
    var v = VIEWS[view] || VIEWS.Default;
    CARD_WIDTH = v.cardWidth;
    CARD_HEIGHT = v.cardHeight;
    COUPLE_GAP = v.coupleGap;
    PORT_INSET = v.portInset;
}

function isBottomToTop(direction) {
    return direction === 'BottomToTop';
}

/**
 * Mirrors the finished layout around the horizontal axis of the canvas.
 *
 * The whole layout is built top-down - dot ranks parents above children, the
 * marriage brackets hang below the cards, the sibling buses run above them - so
 * bottom-to-top is a mirror of the finished geometry rather than a second layout
 * mode. Every wire is orthogonal, so mirroring leaves the corners square, and
 * the cards themselves are only moved, never turned over.
 */
function flipVertical(tree) {
    var i, j;

    for (i = 0; i < tree.children.length; i++) {
        var c = tree.children[i];
        c.y = tree.height - (c.y + c.height);
    }

    for (i = 0; i < tree.edges.length; i++) {
        var s = tree.edges[i].sections[0];
        s.startPoint.y = tree.height - s.startPoint.y;
        s.endPoint.y = tree.height - s.endPoint.y;
        for (j = 0; j < s.bendPoints.length; j++)
            s.bendPoints[j].y = tree.height - s.bendPoints[j].y;
    }
}

/**
 * Sorts the persons into an order that does not depend on the ids.
 *
 * The backend invents a fresh random Guid for every "unknown spouse" placeholder
 * on every run, so an id-derived order makes the layout differ from run to run
 * for no reason. Sorting by name plus the surrounding names pins it down.
 */
function canonicalPersons(persons, relations) {
    var spouses = {}, kids = {}, i;
    var name = {};

    for (i = 0; i < persons.length; i++) name[persons[i].Id] = persons[i].Name || '';

    for (i = 0; i < relations.length; i++) {
        var r = relations[i];
        (spouses[r.From] || (spouses[r.From] = [])).push(name[r.To] || '');
        (spouses[r.To] || (spouses[r.To] = [])).push(name[r.From] || '');
    }
    for (i = 0; i < persons.length; i++) {
        var p = persons[i];
        if (p.Parents) (kids[p.Parents] || (kids[p.Parents] = [])).push(p.Name || '');
    }

    var keyed = [];
    for (i = 0; i < persons.length; i++) {
        var person = persons[i];
        var own = (spouses[person.Id] || []).slice().sort().join(',');
        var below = [];
        for (var j = 0; j < relations.length; j++) {
            var rel = relations[j];
            if (rel.From === person.Id || rel.To === person.Id)
                below = below.concat(kids[rel.Id] || []);
        }
        keyed.push({
            p: person,
            key: [person.Name || '', person.Parents || '', own, below.sort().join(',')].join(''),
            i: i
        });
    }

    keyed.sort(function (a, b) {
        if (a.key !== b.key) return a.key < b.key ? -1 : 1;
        return a.i - b.i;
    });

    var out = [];
    for (i = 0; i < keyed.length; i++) out.push(keyed[i].p);
    return out;
}

// ---------------------------------------------------------------- family blocks

/**
 * Merges every couple into a single wide node. dot's crossing minimisation has
 * no notion of "keep these two nodes adjacent", so without this a stranger's
 * card can end up wedged between two spouses.
 */
function buildBlocks(persons, relations) {
    var parent = {};
    var i;

    for (i = 0; i < persons.length; i++)
        parent[persons[i].Id] = persons[i].Id;

    function find(x) {
        while (parent[x] !== x) {
            parent[x] = parent[parent[x]];
            x = parent[x];
        }
        return x;
    }

    for (i = 0; i < relations.length; i++) {
        var rel = relations[i];
        if (parent[rel.From] === undefined || parent[rel.To] === undefined)
            continue;
        var ra = find(rel.From), rb = find(rel.To);
        if (ra !== rb) parent[ra] = rb;
    }

    var byRoot = {};
    var list = [];
    var blockOf = {};
    for (i = 0; i < persons.length; i++) {
        var root = find(persons[i].Id);
        if (!byRoot[root]) {
            byRoot[root] = { id: root, name: 'b' + list.length, members: [], relations: [] };
            list.push(byRoot[root]);
        }
        byRoot[root].members.push(persons[i].Id);
        blockOf[persons[i].Id] = root;
    }
    for (i = 0; i < relations.length; i++) {
        var b = byRoot[find(relations[i].From)];
        if (b) b.relations.push(relations[i]);
    }

    var byName = {};
    for (i = 0; i < list.length; i++) {
        var block = list[i];
        block.members = chainMembers(block.members, block.relations);
        block.width = block.members.length * CARD_WIDTH + (block.members.length - 1) * COUPLE_GAP;
        block.orders = memberOrders(block);
        byName[block.name] = block;
    }

    return { list: list, byId: byRoot, byName: byName, blockOf: blockOf };
}

/** Orders a block's members along the marriage chain, so spouses touch. */
function chainMembers(members, relations) {
    if (members.length < 2) return members;

    var adj = {};
    var i;
    for (i = 0; i < members.length; i++) adj[members[i]] = [];
    for (i = 0; i < relations.length; i++) {
        var r = relations[i];
        if (adj[r.From] && adj[r.To]) {
            adj[r.From].push(r.To);
            adj[r.To].push(r.From);
        }
    }

    var start = members[0];
    for (i = 0; i < members.length; i++) {
        if (adj[members[i]].length === 1) { start = members[i]; break; }
    }

    var seen = {}, order = [], stack = [start];
    while (stack.length) {
        var id = stack.pop();
        if (seen[id]) continue;
        seen[id] = true;
        order.push(id);
        var next = adj[id];
        for (i = next.length - 1; i >= 0; i--)
            if (!seen[next[i]]) stack.push(next[i]);
    }
    for (i = 0; i < members.length; i++)
        if (!seen[members[i]]) order.push(members[i]);

    return order;
}

/**
 * The candidate left-to-right arrangements of one block's cards.
 *
 * Which spouse stands on which side is free, and picking the wrong side leaves a
 * long diagonal run to the parents. Small blocks get every permutation that
 * keeps married pairs touching; larger ones just get the chain and its mirror.
 */
function memberOrders(block) {
    var members = block.members;
    if (members.length < 2) return [members];
    if (members.length > 4)
        return [members, members.slice().reverse()];

    var married = {};
    var i;
    for (i = 0; i < block.relations.length; i++) {
        var r = block.relations[i];
        married[r.From + ' ' + r.To] = true;
        married[r.To + ' ' + r.From] = true;
    }

    var best = [], bestPairs = -1;
    permute(members, function (order) {
        var pairs = 0;
        for (var j = 1; j < order.length; j++)
            if (married[order[j - 1] + ' ' + order[j]]) pairs++;
        if (pairs > bestPairs) { bestPairs = pairs; best = [order]; }
        else if (pairs === bestPairs) best.push(order);
    });

    return best;
}

function permute(items, fn) {
    var out = [];
    (function step(rest, acc) {
        if (!rest.length) { fn(acc.slice()); return; }
        for (var i = 0; i < rest.length; i++) {
            acc.push(rest[i]);
            step(rest.slice(0, i).concat(rest.slice(i + 1)), acc);
            acc.pop();
        }
    })(items, out);
}

// ---------------------------------------------------------------- context

/** Precomputes the parent/child wiring the scorer needs, in block terms. */
function buildContext(persons, relations, blocks) {
    var relById = {}, i;
    for (i = 0; i < relations.length; i++) relById[relations[i].Id] = relations[i];

    // block -> block links, one per distinct pair, as dot sees them
    var links = [], seen = {};
    // union -> children, and union -> spouses, as the scorer sees them
    var kids = {};

    for (i = 0; i < persons.length; i++) {
        var person = persons[i];
        if (!person.Parents) continue;
        var rel = relById[person.Parents];
        if (!rel) continue;

        (kids[rel.Id] || (kids[rel.Id] = [])).push(person.Id);

        var from = blocks.blockOf[rel.From] || blocks.blockOf[rel.To];
        var to = blocks.blockOf[person.Id];
        if (!from || !to || from === to) continue;

        var key = from + ' ' + to;
        if (seen[key]) continue;    // dot only needs the family link once
        seen[key] = true;
        links.push({ from: blocks.byId[from].name, to: blocks.byId[to].name });
    }

    var ctx = { links: links, kids: kids, relById: relById };
    ctx.kin = buildKin(relations, ctx, blocks);
    return ctx;
}

/**
 * The pairs of blocks that may stand close together.
 *
 * Siblings read as one group and should stay tight; two families with no such
 * link should be visibly apart. Gathering the children per *person* rather than
 * per union means half-siblings, who share only one parent, count as kin too.
 */
function buildKin(relations, ctx, blocks) {
    var byPerson = {}, i, j, k;

    function add(personId, name) {
        if (!byPerson[personId]) byPerson[personId] = [];
        byPerson[personId].push(name);
    }

    for (i = 0; i < relations.length; i++) {
        var rel = relations[i];
        var kids = ctx.kids[rel.Id] || [];
        for (j = 0; j < kids.length; j++) {
            var root = blocks.blockOf[kids[j]];
            if (!root) continue;
            var name = blocks.byId[root].name;
            add(rel.From, name);
            add(rel.To, name);
        }
    }

    var kin = {};
    for (var personId in byPerson) {
        if (!Object.prototype.hasOwnProperty.call(byPerson, personId)) continue;
        var list = byPerson[personId];
        for (j = 0; j < list.length; j++)
            for (k = j + 1; k < list.length; k++)
                if (list[j] !== list[k]) kin[pairKey(list[j], list[k])] = true;
    }
    return kin;
}

function pairKey(a, b) {
    return a < b ? a + ' ' + b : b + ' ' + a;
}

/** The minimum gap the row must leave between two neighbouring blocks. */
function gapBetween(kin, a, b) {
    return kin[pairKey(a.name, b.name)] ? KIN_GAP : STRANGER_GAP;
}

// ---------------------------------------------------------------- dot source

function buildDot(blocks, ctx, blockOrder, linkOrder) {
    var lines = ['digraph G {'];
    lines.push('  graph [rankdir=TB, ranksep="' + RANKSEP + '", nodesep="' + NODESEP + '"];');
    lines.push('  node [shape=box, fixedsize=true, height="' + (CARD_HEIGHT / 72) + '", label=""];');
    lines.push('  edge [arrowhead=none];');

    var i;
    for (i = 0; i < blockOrder.length; i++) {
        var b = blocks.list[blockOrder[i]];
        lines.push('  ' + b.name + ' [width="' + (b.width / 72) + '"];');
    }

    for (i = 0; i < linkOrder.length; i++) {
        var link = ctx.links[linkOrder[i]];
        lines.push('  ' + link.from + ' -> ' + link.to + ';');
    }

    lines.push('}');
    return lines.join('\n');
}

// ---------------------------------------------------------------- search

function mulberry32(seed) {
    var a = seed >>> 0;
    return function () {
        a = (a + 0x6D2B79F5) >>> 0;
        var t = Math.imul(a ^ (a >>> 15), 1 | a);
        t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
        return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
}

function identity(n) {
    var a = [];
    for (var i = 0; i < n; i++) a.push(i);
    return a;
}

function shuffled(n, rnd) {
    var a = identity(n);
    for (var i = n - 1; i > 0; i--) {
        var j = Math.floor(rnd() * (i + 1));
        var t = a[i]; a[i] = a[j]; a[j] = t;
    }
    return a;
}

/**
 * Runs dot over many declaration orders and keeps the tidiest result.
 *
 * dot itself has no thoroughness dial - mclimit, nslimit and searchsize all
 * produce byte-identical output on graphs this shape, because it converges long
 * before those limits bite. The one thing that does move it is the order the
 * graph is handed over in, so that is what gets searched.
 */
function search(viz, persons, relations, blocks, ctx, exhaustive) {
    var rnd = mulberry32(0x5eed);
    var started = Date.now();
    var best = null;

    // Nothing to shuffle: a partial tree (dot handles those in one go), a single
    // block, or a forest of blocks dot will just line up side by side whatever
    // order they arrive in.
    var tries = exhaustive && ctx.links.length && blocks.list.length > 2 ? MAX_ATTEMPTS : 1;

    for (var attempt = 0; attempt < tries; attempt++) {
        if (attempt >= MIN_ATTEMPTS && Date.now() - started > BUDGET_MS)
            break;

        var blockOrder = attempt === 0 ? identity(blocks.list.length) : shuffled(blocks.list.length, rnd);
        var linkOrder = attempt === 0 ? identity(ctx.links.length) : shuffled(ctx.links.length, rnd);

        var json = viz.renderJSON(buildDot(blocks, ctx, blockOrder, linkOrder), { engine: 'dot' });
        var placed = readBlocks(json, blocks);
        if (!placed) continue;

        var rows = buildRows(blocks, placed);
        spread(placed, rows, ctx.kin);
        pickMemberOrders(blocks, placed, ctx);
        compact(blocks, placed, ctx, rows);
        pickMemberOrders(blocks, placed, ctx);

        var pos = expand(blocks, placed, relations);
        var cost = score(pos, relations, ctx, placed);

        if (best === null || cost < best.cost)
            best = { cost: cost, pos: pos };
    }

    if (best === null)
        throw new Error('dot produced no usable layout');

    return best;
}

/** Groups the placed blocks into rows, each sorted left to right. */
function buildRows(blocks, placed) {
    var rows = {}, i;
    for (i = 0; i < blocks.list.length; i++) {
        var block = blocks.list[i];
        var p = placed[block.name];
        if (!p) continue;
        var key = Math.round(p.y);
        if (!rows[key]) rows[key] = [];
        rows[key].push(block);
    }

    var rowList = [];
    for (var key2 in rows) {
        if (!Object.prototype.hasOwnProperty.call(rows, key2)) continue;
        rows[key2].sort(function (a, b) { return placed[a.name].x - placed[b.name].x; });
        rowList.push(rows[key2]);
    }
    return rowList;
}

/**
 * Pushes families that are not siblings apart.
 *
 * dot laid the row out at the tight gap, because nodesep applies to the whole
 * graph and siblings need it tight. This opens up every other neighbouring pair
 * to the wide one. Blocks only ever move right, so the row order that dot chose
 * - and with it the crossing count - survives untouched.
 */
function spread(placed, rowList, kin) {
    for (var i = 0; i < rowList.length; i++) {
        var row = rowList[i];
        for (var j = 1; j < row.length; j++) {
            var prev = row[j - 1], cur = row[j];
            var min = placed[prev.name].x + prev.width + gapBetween(kin, prev, cur);
            if (placed[cur.name].x < min) placed[cur.name].x = min;
        }
    }
}

/**
 * Slides blocks into the slack dot left behind.
 *
 * dot places a block to straighten the block-to-block links it was given, and it
 * knows nothing about the union points - the little junctions the marriage and
 * descent wires actually meet at. So a block often sits several hundred pixels
 * away from where its wires would like it, with an empty gap in between. This
 * walks every block towards the median of its own wire endpoints, never past its
 * neighbours, so the row order, the row width and the crossing count all stay
 * exactly as dot decided them.
 */
function compact(blocks, placed, ctx, rowList) {
    var i, j;

    for (var pass = 0; pass < COMPACT_PASSES; pass++) {
        var moved = false;
        for (i = 0; i < rowList.length; i++) {
            var row = rowList[i];
            // Alternating the sweep direction keeps a block from being blocked
            // for good by a neighbour that has not been pulled along yet.
            var order = identity(row.length);
            if (pass % 2) order.reverse();

            for (j = 0; j < order.length; j++) {
                var b = row[order[j]];
                var target = desiredX(blocks, placed, ctx, b);
                if (target === null) continue;

                var idx = order[j];
                var low = idx > 0
                    ? placed[row[idx - 1].name].x + row[idx - 1].width
                        + gapBetween(ctx.kin, row[idx - 1], b)
                    : -Infinity;
                var high = idx < row.length - 1
                    ? placed[row[idx + 1].name].x - b.width
                        - gapBetween(ctx.kin, b, row[idx + 1])
                    : Infinity;

                var x = Math.min(Math.max(target, low), high);
                if (Math.abs(x - placed[b.name].x) > 0.5) {
                    placed[b.name].x = x;
                    moved = true;
                }
            }
        }
        if (!moved) break;
    }
}

/**
 * The x this block's wires would put it at: the median of one target per wire,
 * which is the point that minimises their total horizontal run.
 */
function desiredX(blocks, placed, ctx, block) {
    var order = block.order || block.members;
    var targets = [];
    var i, j;

    // wires up to each member's own parents
    for (i = 0; i < order.length; i++) {
        var rel = parentRelOf(ctx, order[i]);
        if (!rel) continue;
        var u = unionPoint(blocks, placed, ctx, rel);
        if (u) targets.push(u.x - i * (CARD_WIDTH + COUPLE_GAP) - CARD_WIDTH / 2);
    }

    // wires down from each of the block's own unions to their children
    for (i = 0; i < block.relations.length; i++) {
        var r = block.relations[i];
        var ia = order.indexOf(r.From), ib = order.indexOf(r.To);
        if (ia < 0 && ib < 0) continue;
        var offset = ia >= 0 && ib >= 0
            ? ((ia + ib) / 2) * (CARD_WIDTH + COUPLE_GAP) + CARD_WIDTH / 2
            : Math.max(ia, ib) * (CARD_WIDTH + COUPLE_GAP) + CARD_WIDTH / 2;

        var kids = ctx.kids[r.Id] || [];
        for (j = 0; j < kids.length; j++) {
            var c = cardCentre(blocks, placed, kids[j]);
            if (c) targets.push(c.x - offset);
        }
    }

    if (!targets.length) return null;
    targets.sort(function (a, b) { return a - b; });
    var mid = targets.length >> 1;
    return targets.length % 2
        ? targets[mid]
        : (targets[mid - 1] + targets[mid]) / 2;
}

/** Reads back the block rectangles, in page coordinates. */
function readBlocks(json, blocks) {
    var bb = String(json.bb).split(',');
    var height = parseFloat(bb[3]);
    if (!isFinite(height)) return null;

    var placed = {};
    var objects = json.objects || [];
    for (var i = 0; i < objects.length; i++) {
        var o = objects[i];
        if (!o.pos || o.name === undefined) continue;
        var block = blocks.byName[o.name];
        if (!block) continue;

        var parts = o.pos.split(',');
        placed[block.name] = {
            x: parseFloat(parts[0]) - block.width / 2,
            y: (height - parseFloat(parts[1])) - CARD_HEIGHT / 2
        };
    }
    return placed;
}

/**
 * Chooses, for every block, which of its candidate arrangements puts the fewest
 * long horizontal runs on the wires around it.
 */
function pickMemberOrders(blocks, placed, ctx) {
    for (var i = 0; i < blocks.list.length; i++) {
        var block = blocks.list[i];
        if (block.orders.length < 2) { block.order = block.orders[0]; continue; }
        block.order = block.orders[0];
    }

    // Two sweeps are enough: the first fixes each block against its neighbours'
    // default arrangement, the second against their chosen one.
    for (var pass = 0; pass < 2; pass++) {
        for (var j = 0; j < blocks.list.length; j++) {
            var b = blocks.list[j];
            if (b.orders.length < 2 || !placed[b.name]) continue;

            var bestOrder = b.order, bestCost = Infinity;
            for (var k = 0; k < b.orders.length; k++) {
                b.order = b.orders[k];
                var c = localCost(blocks, placed, ctx, b);
                if (c < bestCost) { bestCost = c; bestOrder = b.orders[k]; }
            }
            b.order = bestOrder;
        }
    }
}

/** Where a person's card lands, given the current arrangement of its block. */
function cardCentre(blocks, placed, personId) {
    var block = blocks.byId[blocks.blockOf[personId]];
    if (!block) return null;
    var p = placed[block.name];
    if (!p) return null;
    var order = block.order || block.members;
    var idx = order.indexOf(personId);
    if (idx < 0) return null;
    return {
        x: p.x + idx * (CARD_WIDTH + COUPLE_GAP) + CARD_WIDTH / 2,
        y: p.y
    };
}

/** Horizontal run of every wire touching the given block. */
function localCost(blocks, placed, ctx, block) {
    var total = 0;
    for (var i = 0; i < block.relations.length; i++)
        total += unionCost(blocks, placed, ctx, block.relations[i]);

    // marriages of the block's members that live in some other block cannot
    // happen - a marriage always merges both spouses into one block - so the
    // only other wires are the ones down to this block's members' own parents.
    var order = block.order || block.members;
    for (var j = 0; j < order.length; j++) {
        var rel = parentRelOf(ctx, order[j]);
        if (!rel) continue;
        var u = unionPoint(blocks, placed, ctx, rel);
        var c = cardCentre(blocks, placed, order[j]);
        if (u && c) total += Math.abs(u.x - c.x);
    }
    return total;
}

var parentRelCache = null;
function parentRelOf(ctx, personId) {
    if (parentRelCache === null || parentRelCache.ctx !== ctx) {
        parentRelCache = { ctx: ctx, map: {} };
        for (var relId in ctx.kids) {
            if (!Object.prototype.hasOwnProperty.call(ctx.kids, relId)) continue;
            var list = ctx.kids[relId];
            for (var i = 0; i < list.length; i++)
                parentRelCache.map[list[i]] = ctx.relById[relId];
        }
    }
    return parentRelCache.map[personId];
}

function unionPoint(blocks, placed, ctx, rel) {
    var a = cardCentre(blocks, placed, rel.From),
        b = cardCentre(blocks, placed, rel.To);
    var one = a || b;
    if (!one) return null;
    return {
        x: a && b ? (a.x + b.x) / 2 : one.x,
        y: (a && b ? Math.max(a.y, b.y) : one.y) + CARD_HEIGHT + UNION_DROP
    };
}

/** Horizontal run of the marriage wires and the descent wires of one union. */
function unionCost(blocks, placed, ctx, rel) {
    var u = unionPoint(blocks, placed, ctx, rel);
    if (!u) return 0;

    var total = 0;
    var a = cardCentre(blocks, placed, rel.From),
        b = cardCentre(blocks, placed, rel.To);
    if (a) total += Math.abs(a.x - u.x);
    if (b) total += Math.abs(b.x - u.x);

    var kids = ctx.kids[rel.Id] || [];
    for (var i = 0; i < kids.length; i++) {
        var c = cardCentre(blocks, placed, kids[i]);
        if (c) total += Math.abs(c.x - u.x);
    }
    return total;
}

/** Turns block rectangles plus chosen arrangements into per-person boxes. */
function expand(blocks, placed, relations) {
    var pos = {};
    var i, j;

    for (i = 0; i < blocks.list.length; i++) {
        var block = blocks.list[i];
        var p = placed[block.name];
        if (!p) continue;
        var order = block.order || block.members;
        var x = p.x;
        for (j = 0; j < order.length; j++) {
            pos[order[j]] = { x: x, y: p.y, width: CARD_WIDTH, height: CARD_HEIGHT };
            x += CARD_WIDTH + COUPLE_GAP;
        }
    }

    // The union point sits centred between the two spouses, just below their row.
    for (i = 0; i < relations.length; i++) {
        var rel = relations[i];
        var a = pos[rel.From], b = pos[rel.To];
        var one = a || b;
        if (!one) continue;
        var cxu = a && b
            ? (a.x + a.width / 2 + b.x + b.width / 2) / 2
            : one.x + one.width / 2;
        var cyu = (a && b ? Math.max(a.y, b.y) : one.y) + CARD_HEIGHT + UNION_DROP;
        pos[rel.Id] = { x: cxu - 0.5, y: cyu - 0.5, width: 1, height: 1 };
    }

    return pos;
}

/** How tangled a candidate layout is: horizontal wire run, plus its width. */
function score(pos, relations, ctx, placed) {
    var total = 0, i, j;

    for (i = 0; i < relations.length; i++) {
        var rel = relations[i];
        var u = pos[rel.Id];
        if (!u) continue;
        var ux = u.x + 0.5;

        var a = pos[rel.From], b = pos[rel.To];
        if (a) total += Math.abs(a.x + a.width / 2 - ux);
        if (b) total += Math.abs(b.x + b.width / 2 - ux);

        var kids = ctx.kids[rel.Id] || [];
        for (j = 0; j < kids.length; j++) {
            var c = pos[kids[j]];
            if (!c) continue;
            total += Math.abs(c.x + c.width / 2 - ux) + Math.abs(c.y - u.y);
        }
    }

    var minX = Infinity, maxX = -Infinity;
    for (var id in pos) {
        if (!Object.prototype.hasOwnProperty.call(pos, id)) continue;
        if (pos[id].x < minX) minX = pos[id].x;
        if (pos[id].x + pos[id].width > maxX) maxX = pos[id].x + pos[id].width;
    }

    return total + WIDTH_WEIGHT * (maxX - minX);
}

/**
 * Slides each union point sideways to line up with its children.
 *
 * The point starts centred between the two spouses, so unless the children
 * happen to be centred there too the descent wire leaves with a short sideways
 * jog - and a jog of a dozen pixels reads as a kink rather than as a step.
 *
 * The point is free to sit anywhere between the two cards' centres: it lives
 * below both cards, so even at the extremes it only turns one marriage wire
 * into a straight drop and lengthens the other. That trade is worth it - on the
 * reference tree this both removes every sub-40px kink and shortens the total
 * wire run. Runs after the search, on the winning layout only; the search itself
 * keeps the simpler centred model, which this can only improve on.
 */
function alignUnions(pos, relations, ctx) {
    for (var i = 0; i < relations.length; i++) {
        var rel = relations[i];
        var u = pos[rel.Id], a = pos[rel.From], b = pos[rel.To];
        if (!u || !a || !b) continue;

        var kids = ctx.kids[rel.Id] || [];
        var sum = 0, n = 0;
        for (var j = 0; j < kids.length; j++) {
            var c = pos[kids[j]];
            if (!c) continue;
            sum += c.x + c.width / 2;
            n++;
        }
        if (!n) continue;

        var lo = Math.min(a.x, b.x) + CARD_WIDTH / 2,    // centre of the left card
            hi = Math.max(a.x, b.x) + CARD_WIDTH / 2;    // centre of the right card
        if (hi < lo) continue;                           // spouses not side by side

        u.x = Math.min(Math.max(sum / n, lo), hi) - u.width / 2;
    }
}

function normalize(pos) {
    var minX = Infinity, minY = Infinity, id;
    for (id in pos) {
        if (!Object.prototype.hasOwnProperty.call(pos, id)) continue;
        if (pos[id].x < minX) minX = pos[id].x;
        if (pos[id].y < minY) minY = pos[id].y;
    }
    if (!isFinite(minX)) return;
    for (id in pos) {
        if (!Object.prototype.hasOwnProperty.call(pos, id)) continue;
        pos[id].x -= minX - SPACING;
        pos[id].y -= minY - SPACING;
    }
}

function measure(pos) {
    var maxX = 0, maxY = 0;
    for (var id in pos) {
        if (!Object.prototype.hasOwnProperty.call(pos, id)) continue;
        var p = pos[id];
        if (p.x + p.width > maxX) maxX = p.x + p.width;
        if (p.y + p.height > maxY) maxY = p.y + p.height;
    }
    return { width: Math.ceil(maxX + SPACING), height: Math.ceil(maxY + SPACING) };
}

function buildChildren(persons, relations, pos) {
    var children = [];
    var i;

    for (i = 0; i < persons.length; i++) {
        var person = persons[i];
        var p = pos[person.Id];
        if (!p) continue;
        children.push({
            id: person.Id,
            label: person.Name,
            x: p.x, y: p.y, width: p.width, height: p.height,
            info: person
        });
    }

    // Union nodes carry no `info`, which is how the front-end tells them apart
    // from cards (see convertPersons / detectChildren in tree.js).
    for (i = 0; i < relations.length; i++) {
        var u = pos[relations[i].Id];
        if (!u) continue;
        children.push({
            id: relations[i].Id,
            x: u.x, y: u.y, width: 1, height: 1
        });
    }

    return children;
}

// ---------------------------------------------------------------- routing

/** Groups cards into per-row bands, so trunks can be steered around them. */
function buildBands(pos, persons) {
    var rows = {};
    for (var i = 0; i < persons.length; i++) {
        var p = pos[persons[i].Id];
        if (!p) continue;
        var key = Math.round(p.y);
        if (!rows[key]) rows[key] = { y1: p.y, y2: p.y + p.height, spans: [] };
        if (p.y + p.height > rows[key].y2) rows[key].y2 = p.y + p.height;
        rows[key].spans.push({ id: persons[i].Id, x1: p.x, x2: p.x + p.width });
    }

    var bands = [];
    for (var key2 in rows) {
        if (!Object.prototype.hasOwnProperty.call(rows, key2)) continue;
        rows[key2].spans.sort(function (a, b) { return a.x1 - b.x1; });
        bands.push(rows[key2]);
    }
    bands.sort(function (a, b) { return a.y1 - b.y1; });
    return bands;
}

function isFree(band, x, exceptId) {
    for (var i = 0; i < band.spans.length; i++) {
        var s = band.spans[i];
        if (s.id === exceptId) continue;
        if (x > s.x1 && x < s.x2) return false;
    }
    return true;
}

/** Nearest x to wantX that clears every card in the band. */
function freeX(band, wantX, exceptId) {
    var spans = [];
    var i;
    for (i = 0; i < band.spans.length; i++)
        if (band.spans[i].id !== exceptId) spans.push(band.spans[i]);

    if (!spans.length || isFree(band, wantX, exceptId)) return wantX;

    var gaps = [{ from: -Infinity, to: spans[0].x1 - CARD_CLEARANCE }];
    for (i = 1; i < spans.length; i++) {
        var from = spans[i - 1].x2 + CARD_CLEARANCE, to = spans[i].x1 - CARD_CLEARANCE;
        if (to > from) gaps.push({ from: from, to: to });
    }
    gaps.push({ from: spans[spans.length - 1].x2 + CARD_CLEARANCE, to: Infinity });

    var best = wantX, bestDist = Infinity;
    for (i = 0; i < gaps.length; i++) {
        var c = Math.min(Math.max(wantX, gaps[i].from), gaps[i].to);
        var d = Math.abs(c - wantX);
        if (d < bestDist) { bestDist = d; best = c; }
    }
    return best;
}

/**
 * Walks a trunk down from (x, yFrom) to yTo, stepping sideways into a gap
 * whenever a row of cards is in the way: a line may cross another line, but
 * never a card.
 */
function descend(bands, x, yFrom, yTo, wantX, exceptId) {
    var pts = [[x, yFrom]];
    var cur = x;

    for (var i = 0; i < bands.length; i++) {
        var band = bands[i];
        if (band.y2 <= yFrom + 0.5 || band.y1 >= yTo - 0.5) continue;
        if (isFree(band, cur, exceptId)) continue;

        var lane = freeX(band, wantX, exceptId);
        var laneY = band.y1 - BUS_RISE;
        pts.push([cur, laneY]);
        pts.push([lane, laneY]);
        cur = lane;
    }

    pts.push([cur, yTo]);
    return pts;
}

function routeEdges(persons, relations, pos, bands) {
    var edges = [];
    var i, j;

    // --- marriage lines: person -> union ---------------------------------
    // Someone with several marriages gets one exit port per marriage, spread
    // across the bottom of the card and ordered by which side the union is on.
    var bySpouse = {};
    for (i = 0; i < relations.length; i++) {
        var rel = relations[i];
        pushSpouse(rel.From, rel);
        pushSpouse(rel.To, rel);
    }

    function pushSpouse(personId, rel) {
        if (!pos[personId] || !pos[rel.Id]) return;
        if (!bySpouse[personId]) bySpouse[personId] = [];
        bySpouse[personId].push(rel);
    }

    for (var personId in bySpouse) {
        if (!Object.prototype.hasOwnProperty.call(bySpouse, personId)) continue;
        var card = pos[personId];
        var rels = bySpouse[personId];

        rels.sort(function (a, b) { return pos[a.Id].x - pos[b.Id].x; });

        var usable = card.width - PORT_INSET * 2;
        for (j = 0; j < rels.length; j++) {
            var r = rels[j];
            var px = rels.length === 1
                ? card.x + card.width / 2
                : card.x + PORT_INSET + usable * (j + 1) / (rels.length + 1);

            var u = pos[r.Id];
            var ux = u.x + u.width / 2, uy = u.y + u.height / 2;

            var pts;
            if (uy >= card.y + card.height)
                pts = descend(bands, px, card.y + card.height, uy, ux, personId).concat([[ux, uy]]);
            else
                pts = [[px, card.y], [px, uy], [ux, uy]];

            // The wire stops exactly on the union point, so the front-end must
            // not trim its last segment - doing so would leave a childless
            // marriage as two stubs that never meet.
            edges.push(makeEdge(
                r.Id + ':' + personId,
                [personId],
                [r.Id + ':n'],
                false,
                pts
            ));
        }
    }

    // --- descent lines: union -> children --------------------------------
    var byUnion = {};
    for (i = 0; i < persons.length; i++) {
        var person = persons[i];
        if (!person.Parents || !pos[person.Parents] || !pos[person.Id]) continue;
        if (!byUnion[person.Parents]) byUnion[person.Parents] = [];
        byUnion[person.Parents].push(person);
    }

    for (var unionId in byUnion) {
        if (!Object.prototype.hasOwnProperty.call(byUnion, unionId)) continue;
        var un = pos[unionId];
        var unx = un.x + un.width / 2, uny = un.y + un.height / 2;

        // Children that ended up on different rows each get their own bus; a
        // single shared bus would leave the lower ones hanging off a drop that
        // has to cross whatever sits in between.
        var rowsOf = {};
        var kids = byUnion[unionId];
        for (i = 0; i < kids.length; i++) {
            var kp = pos[kids[i].Id];
            var rowKey = Math.round(kp.y);
            if (!rowsOf[rowKey]) rowsOf[rowKey] = [];
            rowsOf[rowKey].push(kids[i]);
        }

        for (var rowKey2 in rowsOf) {
            if (!Object.prototype.hasOwnProperty.call(rowsOf, rowKey2)) continue;
            var group = rowsOf[rowKey2];
            var rowY = pos[group[0].Id].y;
            var busY = (rowY - BUS_RISE > uny) ? rowY - BUS_RISE : uny;

            var centre = 0;
            for (i = 0; i < group.length; i++)
                centre += pos[group[i].Id].x + CARD_WIDTH / 2;
            centre /= group.length;

            // The sideways step of a lone child lands halfway down the band,
            // UNION_DROP below the union and BUS_RISE above the card, so it
            // reads as a deliberate step rather than a notch against the card.
            var trunk = descend(bands, unx, uny, busY, centre, null);

            for (i = 0; i < group.length; i++) {
                var child = group[i];
                var cp = pos[child.Id];
                var ccx = cp.x + cp.width / 2;
                edges.push(makeEdge(
                    child.Id + ':' + unionId,
                    [unionId + ':s'],
                    [child.Id + ':n'],
                    false,
                    trunk.concat([[ccx, busY], [ccx, cp.y]])
                ));
            }
        }
    }

    return edges;
}

/** Packs a polyline into the section shape the front-end expects. */
function makeEdge(id, sources, targets, fakeTarget, points) {
    var clean = [];
    for (var i = 0; i < points.length; i++) {
        var last = clean[clean.length - 1];
        if (!last || last[0] !== points[i][0] || last[1] !== points[i][1])
            clean.push(points[i]);
    }
    if (clean.length < 2) clean.push([clean[0][0], clean[0][1]]);

    // Drop points that sit on the straight line between their neighbours: the
    // trunk builder emits one per row it walks past, and a redundant bend makes
    // a straight run look like a decision.
    for (var k = clean.length - 2; k > 0; k--) {
        var prev = clean[k - 1], cur = clean[k], next = clean[k + 1];
        if ((prev[0] === cur[0] && cur[0] === next[0]) ||
            (prev[1] === cur[1] && cur[1] === next[1]))
            clean.splice(k, 1);
    }

    var bends = [];
    for (var j = 1; j < clean.length - 1; j++)
        bends.push({ x: clean[j][0], y: clean[j][1] });

    return {
        id: id,
        sources: sources,
        targets: targets,
        info: { fakeTarget: fakeTarget },
        sections: [{
            id: id + '_s0',
            startPoint: { x: clean[0][0], y: clean[0][1] },
            endPoint: { x: clean[clean.length - 1][0], y: clean[clean.length - 1][1] },
            bendPoints: bends
        }]
    };
}
