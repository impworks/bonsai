using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Bonsai.Data.Models;
using Impworks.Utils.Linq;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    /// <summary>
    /// Background job for recalculating the entire tree's layout.
    /// </summary>
    public partial class TreeLayoutJob
    {
        protected async Task ProcessFullTreeAsync(RelationContext ctx, CancellationToken token)
        {
            if (_config.TreeKinds.HasFlag(TreeKind.FullTree) == false)
                return;

            var trees = GetAllSubtrees(ctx);
            var thoroughness = GetThoroughness();

            _logger.Information($"Full tree layout started: {ctx.Pages.Count} people, {ctx.Relations.Count} rels, {trees.Count} subtrees.");

            foreach (var tree in trees)
            {
                var rendered = await RenderTreeAsync(tree, thoroughness, token);
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
            var visited = new HashSet<string>();
            var result = new List<TreeLayoutVM>();

            foreach (var pageId in ctx.Pages.Keys)
            {
                if (visited.Contains(pageId.ToString()))
                    continue;

                var subtree = GetSubtree(ctx, pageId, _ => TraverseMode.Normal);
                result.Add(subtree);

                foreach (var person in subtree.Persons)
                    visited.Add(person.Id);
            }

            return result;
        }

        /// <summary>
        /// Returns interpolated thoroughness.
        /// </summary>
        private int GetThoroughness()
        {
            return Interpolator.MapValue(
                _config.TreeRenderThoroughness,
                new IntervalMap(1, 10, 1, 10),
                new IntervalMap(11, 50, 11, 600),
                new IntervalMap(51, 100, 601, 15000)
            );
        }

        #endregion

        #region Database processing

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
