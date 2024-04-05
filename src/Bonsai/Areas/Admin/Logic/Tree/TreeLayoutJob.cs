using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Localization;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Jering.Javascript.NodeJS;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    public partial class TreeLayoutJob: JobBase
    {
        public TreeLayoutJob(AppDbContext db, INodeJSService js, BonsaiConfigService config, ILogger logger)
        {
            _db = db;
            _js = js;
            _config = config.GetDynamicConfig();
            _logger = logger;
        }

        private readonly AppDbContext _db;
        private readonly INodeJSService _js;
        private readonly DynamicConfig _config;
        private readonly ILogger _logger;

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await _db.Pages.ExecuteUpdateAsync(x => x.SetProperty(p => p.TreeLayoutId, (Guid?)null), token);
            await _db.TreeLayouts.ExecuteDeleteAsync(token);

            var opts = new RelationContextOptions { PeopleOnly = true, TreeRelationsOnly = true };
            var ctx = await RelationContext.LoadContextAsync(_db, opts);

            await ProcessPartialTreeAsync(TreeKind.CloseFamily, GetCloseFamilyTree, ctx, token);
            await ProcessPartialTreeAsync(TreeKind.Ancestors, GetAncestorsTree, ctx, token);
            await ProcessPartialTreeAsync(TreeKind.Descendants, GetDescendantsTree, ctx, token);

            await ProcessFullTreeAsync(ctx, token);
        }

        /// <summary>
        /// Renders the tree using ELK.js.
        /// </summary>
        protected async Task<string> RenderTreeAsync(TreeLayoutVM tree, int thoroughness, CancellationToken token)
        {
            var json = JsonConvert.SerializeObject(tree);
            var result = await _js.InvokeFromFileAsync<string>(
                "./External/tree/tree-layout.js",
                args: [json, thoroughness],
                cancellationToken: token
            );

            if (string.IsNullOrEmpty(result) || result == "null")
                throw new Exception("Failed to render tree: output is empty.");

            return result;
        }

        /// <summary>
        /// Returns the photo for a card, depending on the gender, reverting to a default one if unspecified.
        /// </summary>
        protected string GetPhoto(string actual, bool gender)
        {
            var defaultPhoto = gender
                ? "~/assets/img/unknown-male.png"
                : "~/assets/img/unknown-female.png";

            return StringHelper.Coalesce(
                MediaPresenterService.GetSizedMediaPath(actual, MediaSize.Small),
                defaultPhoto
            );
        }

        /// <summary>
        /// Returns a subtree around a page using the relation filter.
        /// </summary>
        protected TreeLayoutVM GetSubtree(RelationContext context, Guid rootId, Func<RelationFilterContext, TraverseMode?> relFilter, TraverseMode initialMode = TraverseMode.Normal)
        {
            var danglingParents = new HashSet<Guid>();
            var parentKeys = new HashSet<string>();

            var persons = new Dictionary<Guid, TreePersonVM>();
            var relations = new Dictionary<string, TreeRelationVM>();

            var pending = new Queue<Step>();
            pending.Enqueue(new Step(rootId, initialMode, 0, null));

            while (pending.TryDequeue(out var step))
            {
                if (persons.ContainsKey(step.PageId))
                    continue;

                if (!context.Pages.TryGetValue(step.PageId, out var page))
                    continue;

                AddPage(page, step.Mode);

                if (step.Mode.HasFlag(TraverseMode.TraverseRelations) && context.Relations.TryGetValue(step.PageId, out var rels))
                {
                    foreach (var rel in rels)
                    {
                        var relMode = relFilter(new RelationFilterContext(step.PageId, step.Distance + 1, rel.Type, step.LastRelationType));
                        if(relMode == null)
                            continue;

                        pending.Enqueue(new Step(rel.DestinationId, relMode.Value, step.Distance + 1, rel.Type));

                        if (rel.Type == RelationType.Spouse)
                            AddRelationship(page.Id, rel.DestinationId);
                    }
                }
            }

            foreach (var parentId in danglingParents)
            {
                if (persons.ContainsKey(parentId))
                    continue;

                if (!context.Pages.TryGetValue(parentId, out var page))
                    continue;

                AddPage(page, TraverseMode.DeadEnd);
            }

            return new TreeLayoutVM
            {
                PageId = rootId,
                Persons = persons.Values.OrderBy(x => x.Name).ToList(),
                Relations = relations.Values.OrderBy(x => x.Id).ToList()
            };

            void AddPage(RelationContext.PageExcerpt page, TraverseMode mode)
            {
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
                        Parents = mode.HasFlag(TraverseMode.SetParents)
                            ? GetParentRelationshipId(page)
                            : null
                    }
                );
            }

            string GetParentRelationshipId(RelationContext.PageExcerpt page)
            {
                if (!context.Relations.TryGetValue(page.Id, out var allRels))
                    return null;

                var rels = allRels.Where(x => x.Type == RelationType.Parent).ToList();
                if (rels.Count == 0)
                    return null;

                danglingParents.AddRange(rels.Select(x => x.DestinationId));

                var relKey = rels.Count == 1
                    ? rels[0].DestinationId + ":unknown"
                    : rels.Select(x => x.DestinationId.ToString()).OrderBy(x => x).JoinString(":");

                if (!parentKeys.Contains(relKey))
                {
                    if (rels.Count == 1)
                    {
                        var fakeId = Guid.NewGuid();
                        var relPage = context.Pages[rels[0].DestinationId];
                        var fakeGender = !(relPage.Gender ?? true);
                        persons.Add(fakeId, new TreePersonVM
                        {
                            Id = fakeId.ToString(),
                            Name = Texts.Admin_Tree_Unknown,
                            IsMale = fakeGender,
                            Photo = GetPhoto(null, fakeGender)
                        });

                        AddRelationship(rels[0].DestinationId, fakeId, relKey);
                    }
                    else
                    {
                        AddRelationship(rels[0].DestinationId, rels[1].DestinationId);
                    }

                    parentKeys.Add(relKey);
                }

                return relKey;
            }

            void AddRelationship(Guid r1, Guid r2, string keyOverride = null)
            {
                var from = r1.ToString();
                var to = r2.ToString();
                if (from.CompareTo(to) >= 1)
                    (from, to) = (to, from);

                var key = StringHelper.Coalesce(keyOverride, from + ":" + to);
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

        [Flags]
        protected enum TraverseMode
        {
            DeadEnd = 0,
            TraverseRelations = 1,
            SetParents = 2,
            Normal = 3
        }

        protected record struct Step(Guid PageId, TraverseMode Mode, int Distance, RelationType? LastRelationType);
        protected record struct RelationFilterContext(Guid PageId, int Distance, RelationType RelationType, RelationType? LastRelationType);
    }
}
