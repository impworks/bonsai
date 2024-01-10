using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    /// <summary>
    /// Background job for recalculating the partial trees.
    /// </summary>
    public partial class TreeLayoutJob
    {
        /// <summary>
        /// Processes the layouts for a particular tree kind.
        /// </summary>
        private async Task ProcessPartialTreeAsync(TreeKind kind, Func<RelationContext, Guid, TreeLayoutVM> treeGetter, RelationContext ctx, CancellationToken token)
        {
            if (_config.TreeKinds.HasFlag(kind) == false)
                return;

            _logger.Information($"Partial tree ({kind}) layouts started: {ctx.Pages.Count} subtrees.");

            var layouts = new List<TreeLayout>();
            foreach (var page in ctx.Pages.Values)
            {
                var tree = treeGetter(ctx, page.Id);
                var rendered = await RenderTreeAsync(tree, 600, token);
                layouts.Add(new TreeLayout
                {
                    Id = Guid.NewGuid(),
                    LayoutJson = rendered,
                    GenerationDate = DateTimeOffset.Now,
                    PageId = tree.PageId,
                    Kind = kind
                });
            }

            await _db.TreeLayouts.Where(x => x.Kind == kind).ExecuteDeleteAsync(CancellationToken.None);

            _db.TreeLayouts.AddRange(layouts);
            await _db.SaveChangesAsync(CancellationToken.None);

            _logger.Information($"Partial tree ({kind}) layouts completed.");
        }

        /// <summary>
        /// Finds close family members for the page.
        /// </summary>
        private TreeLayoutVM GetCloseFamilyTree(RelationContext ctx, Guid pageId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds ancestors for the page.
        /// </summary>
        private TreeLayoutVM GetAncestorsTree(RelationContext ctx, Guid pageId)
        {
            return GetSubtree(ctx, pageId, r => r.Type == RelationType.Parent);
        }

        /// <summary>
        /// Finds descendants for the page.
        /// </summary>
        private TreeLayoutVM GetDescendantsTree(RelationContext ctx, Guid pageId)
        {
            return GetSubtree(ctx, pageId, rel => rel.Type == RelationType.Child);
        }
    }
}
