using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Jering.Javascript.NodeJS;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    /// <summary>
    /// Background job for recalculating the entire tree's layout.
    /// </summary>
    public class EntireTreeLayoutJob : TreeLayoutJobBase
    {
        public EntireTreeLayoutJob(AppDbContext db, INodeJSService js, BonsaiConfigService config, ILogger logger)
            : base(db, js, config, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await FlushTreeAsync(_db);

            if (_config.GetDynamicConfig().TreeKinds.HasFlag(TreeKind.FullTree) == false)
                return;

            var hasPages = await _db.Pages.AnyAsync();
            if (!hasPages)
                return;

            var ctx = await GetRelationContextAsync();
            var trees = GetAllSubtrees(ctx);
            var thoroughness = GetThoroughness();

            _logger.Information($"Full tree layout started: {ctx.Pages.Count} people, {ctx.Relations.Count} rels, {trees.Count} subtrees.");

            foreach (var tree in trees)
            {
                var rendered = await RenderTree(tree, thoroughness, token);
                var layout = new TreeLayout
                {
                    Id = Guid.NewGuid(),
                    LayoutJson = rendered,
                    GenerationDate = DateTimeOffset.Now
                };

                await SaveLayoutAsync(_db, tree, layout);
            }

            _logger.Information("Full tree layout completed.");
        }

        #region Tree generation

        /// <summary>
        /// Loads all pages and groups them into subgraphs by relations.
        /// </summary>
        private IReadOnlyList<TreeLayoutVM> GetAllSubtrees(RelationContext ctx)
        {
            var excluded = new HashSet<Guid>();

            var result = new List<TreeLayoutVM>();

            while (true)
            {
                var root = ctx.Pages.Keys.FirstOrDefault(x => !excluded.Contains(x));
                if (root == Guid.Empty)
                    break;

                result.Add(GetSubtree(ctx, excluded, root));
            }

            return result;
        }

        /// <summary>
        /// Returns the entire tree.
        /// </summary>
        private TreeLayoutVM GetSubtree(RelationContext context, HashSet<Guid> visited, Guid rootId)
        {
            var parents = new HashSet<string>();

            var persons = new Dictionary<Guid, TreePersonVM>();
            var relations = new Dictionary<string, TreeRelationVM>();

            var pending = new Queue<Guid>();
            pending.Enqueue(rootId);

            while (pending.TryDequeue(out var currId))
            {
                if (visited.Contains(currId))
                    continue;

                if (!context.Pages.TryGetValue(currId, out var page))
                    continue;

                persons.Add(
                    page.Id,
                    new TreePersonVM
                    {
                        Id = page.Id.ToString(),
                        Name = page.Title,
                        MaidenName = page.MaidenName,
                        Birth = page.BirthDate?.ShortReadableDate,
                        Death = page.DeathDate?.ShortReadableDate,
                        IsMale = page.Gender ?? true,
                        IsDead = page.IsDead,
                        Photo = GetPhoto(page.MainPhotoPath, page.Gender ?? true),
                        Url = page.Key,
                        Parents = GetParentRelationshipId(page)
                    }
                );

                visited.Add(currId);

                if (context.Relations.TryGetValue(currId, out var rels))
                {
                    foreach (var rel in rels)
                    {
                        if (rel.Type != RelationType.Child && rel.Type != RelationType.Parent && rel.Type != RelationType.Spouse)
                            continue;

                        pending.Enqueue(rel.DestinationId);

                        if (rel.Type == RelationType.Spouse)
                            AddRelationship(page.Id, rel.DestinationId);
                    }
                }
            }

            return new TreeLayoutVM
            {
                Persons = persons.Values.OrderBy(x => x.Name).ToList(),
                Relations = relations.Values.OrderBy(x => x.Id).ToList()
            };

            string GetParentRelationshipId(RelationContext.PageExcerpt page)
            {
                if (!context.Relations.TryGetValue(page.Id, out var allRels))
                    return null;

                var rels = allRels.Where(x => x.Type == RelationType.Parent).ToList();
                if (rels.Count == 0)
                    return null;

                var relKey = rels.Count == 1
                    ? rels[0].DestinationId + ":unknown"
                    : rels.Select(x => x.DestinationId.ToString()).OrderBy(x => x).JoinString(":");

                if (!parents.Contains(relKey))
                {
                    if (rels.Count == 1)
                    {
                        var fakeId = Guid.NewGuid();
                        var relPage = context.Pages[rels[0].DestinationId];
                        var fakeGender = !(relPage.Gender ?? true);
                        persons.Add(fakeId, new TreePersonVM
                        {
                            Id = fakeId.ToString(),
                            Name = "Неизвестно",
                            IsMale = fakeGender,
                            Photo = GetPhoto(null, fakeGender)
                        });

                        AddRelationship(rels[0].DestinationId, fakeId, relKey);
                    }
                    else
                    {
                        AddRelationship(rels[0].DestinationId, rels[1].DestinationId);
                    }

                    parents.Add(relKey);
                }

                return relKey;
            }

            void AddRelationship(Guid r1, Guid r2, string keyOverride = null)
            {
                var from = r1.ToString();
                var to = r2.ToString();
                if (from.CompareTo(to) >= 1)
                    (from, to) = (to, from);

                var key = keyOverride;
                if (string.IsNullOrEmpty(key))
                    key = from + ":" + to;

                if (relations.ContainsKey(key))
                    return;

                relations.Add(key, new TreeRelationVM
                {
                    Id = key,
                    From = from,
                    To = to
                });
            }
        }

        /// <summary>
        /// Returns interpolated thoroughness.
        /// </summary>
        private int GetThoroughness()
        {
            var cfg = _config.GetDynamicConfig();
            return Interpolator.MapValue(
                cfg.TreeRenderThoroughness,
                new IntervalMap(1, 10, 1, 10),
                new IntervalMap(11, 50, 11, 600),
                new IntervalMap(51, 100, 601, 15000)
            );
        }

        #endregion

        #region Database processing

        /// <summary>
        /// Removes all existing tree layouts.
        /// </summary>
        private async Task FlushTreeAsync(AppDbContext db)
        {
            await db.Pages.ExecuteUpdateAsync(x => x.SetProperty(p => p.TreeLayoutId, (Guid?)null));
            await db.TreeLayouts.ExecuteDeleteAsync();
        }

        /// <summary>
        /// Updates the layout.
        /// </summary>
        private async Task SaveLayoutAsync(AppDbContext db, TreeLayoutVM tree, TreeLayout layout)
        {
            db.TreeLayouts.Add(layout);
            await db.SaveChangesAsync();

            foreach (var batch in tree.Persons.Select(x => Guid.Parse(x.Id)).PartitionBySize(100))
            {
                await db.Pages
                        .Where(x => batch.Contains(x.Id))
                        .ExecuteUpdateAsync(x => x.SetProperty(p => p.TreeLayoutId, layout.Id));
            }
        }

        #endregion
    }
}
