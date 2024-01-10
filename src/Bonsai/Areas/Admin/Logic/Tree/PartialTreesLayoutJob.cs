using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services.Config;
using Bonsai.Data;
using Bonsai.Data.Models;
using Jering.Javascript.NodeJS;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    /// <summary>
    /// Background job for recalculating the partial trees.
    /// </summary>
    public class PartialTreesLayoutJob: TreeLayoutJobBase
    {
        public PartialTreesLayoutJob(AppDbContext db, INodeJSService js, BonsaiConfigService config, ILogger logger)
            : base(db, js, config, logger)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            var ctx = await GetRelationContextAsync();

            await ProcessTreeKindAsync(TreeKind.CloseFamily, GetCloseFamilyTree, ctx, token);
            await ProcessTreeKindAsync(TreeKind.Ancestors, GetAncestorsTree, ctx, token);
            await ProcessTreeKindAsync(TreeKind.Descendants, GetDescendantsTree, ctx, token);
        }

        /// <summary>
        /// Processes the layouts for a particular tree kind.
        /// </summary>
        private async Task ProcessTreeKindAsync(TreeKind kind, Func<RelationContext, RelationContext.PageExcerpt, TreeLayoutVM> treeGetter, RelationContext ctx, CancellationToken token)
        {
            var config = _config.GetDynamicConfig();
            if (config.TreeKinds.HasFlag(kind) == false)
                return;

            _logger.Information($"Partial tree ({kind}) layouts started: {ctx.Pages.Count} subtrees.");

            var layouts = new List<TreeLayout>();
            foreach (var page in ctx.Pages.Values)
            {
                var tree = treeGetter(ctx, page);
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
        private TreeLayoutVM GetCloseFamilyTree(RelationContext ctx, RelationContext.PageExcerpt page)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds ancestors for the page.
        /// </summary>
        private TreeLayoutVM GetAncestorsTree(RelationContext ctx, RelationContext.PageExcerpt page)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds descendants for the page.
        /// </summary>
        private TreeLayoutVM GetDescendantsTree(RelationContext ctx, RelationContext.PageExcerpt page)
        {
            throw new NotImplementedException();
        }
    }
}
