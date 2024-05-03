using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Bonsai.Areas.Admin.Logic.Tree;

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

            try
            {
                var rendered = await RenderTreeAsync(tree, 1000, token);

                layouts.Add(new TreeLayout
                {
                    Id = Guid.NewGuid(),
                    LayoutJson = rendered,
                    GenerationDate = DateTimeOffset.Now,
                    PageId = tree.PageId,
                    Kind = kind
                });
            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(tree);
                _logger.Error(ex.Demystify(), $"Failed to render partial tree ({kind}) for page {page.Id}:\n{json}");
            }
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
        return GetSubtree(
            ctx,
            pageId,
            fc =>
            {
                // explicitly avoid e.g. other husbands of a man's wife
                if (fc is {Distance: 2, RelationType: RelationType.Spouse, LastRelationType: RelationType.Spouse})
                    return null;

                return fc.Distance switch
                {
                    < 2 => TraverseMode.Normal,
                    2 => fc.RelationType == RelationType.Child
                        ? TraverseMode.SetParents
                        : TraverseMode.DeadEnd,
                    _ => null
                };
            });
    }

    /// <summary>
    /// Finds ancestors for the page.
    /// </summary>
    private TreeLayoutVM GetAncestorsTree(RelationContext ctx, Guid pageId)
    {
        return GetSubtree(
            ctx,
            pageId,
            fc => fc.RelationType == RelationType.Parent
                ? TraverseMode.Normal
                : null
        );
    }

    /// <summary>
    /// Finds descendants for the page.
    /// </summary>
    private TreeLayoutVM GetDescendantsTree(RelationContext ctx, Guid pageId)
    {
        return GetSubtree(
            ctx,
            pageId,
            fc =>
            {
                if (fc.RelationType == RelationType.Child)
                    return TraverseMode.Normal;

                if (fc.RelationType == RelationType.Parent && fc.PageId != pageId)
                    return TraverseMode.DeadEnd;

                return null;
            },
            TraverseMode.TraverseRelations
        );
    }
}