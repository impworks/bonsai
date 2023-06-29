﻿using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.ViewModels.Tree;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Dapper;
using Impworks.Utils.Linq;
using Impworks.Utils.Strings;
using Jering.Javascript.NodeJS;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;

namespace Bonsai.Areas.Admin.Logic.Tree
{
    /// <summary>
    /// Background job for recalculating the entire tree's layout.
    /// </summary>
    public class EntireTreeLayoutJob : JobBase
    {
        private readonly AppDbContext _db;
        private readonly INodeJSService _js;
        private readonly BonsaiConfigService _config;
        private readonly ILogger _logger;

        public EntireTreeLayoutJob(AppDbContext db, INodeJSService js, BonsaiConfigService config, ILogger logger)
        {
            _db = db;
            _js = js;
            _config = config;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken token)
        {
            await FlushTreeAsync(_db);

            var hasPages = await _db.Pages.AnyAsync(x => x.TreeLayoutId == null);
            if (!hasPages)
                return;

            var opts = new RelationContextOptions { PeopleOnly = true };
            var ctx = await RelationContext.LoadContextAsync(_db, opts);
            var trees = GetAllSubtrees(ctx);

            _logger.Information($"Tree layout started: {ctx.Pages.Count} people, {ctx.Relations.Count} rels, {trees.Count} subtrees.");

            foreach (var tree in trees)
            {
                var rendered = await RenderTree(_js, tree, _config.GetDynamicConfig(), token);
                var layout = new TreeLayout
                {
                    Id = Guid.NewGuid(),
                    LayoutJson = rendered,
                    GenerationDate = DateTimeOffset.Now
                };

                await SaveLayoutAsync(_db, tree, layout);
            }

            _logger.Information("Tree layout completed.");
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
        /// Returns the photo for a card, depending on the gender, reverting to a default one if unspecified.
        /// </summary>
        private string GetPhoto(string actual, bool gender)
        {
            var defaultPhoto = gender
                ? "~/assets/img/unknown-male.png"
                : "~/assets/img/unknown-female.png";

            return StringHelper.Coalesce(
                MediaPresenterService.GetSizedMediaPath(actual, MediaSize.Small),
                defaultPhoto
            );
        }

        #endregion

        #region Tree rendering

        /// <summary>
        /// Renders the tree using ELK.js.
        /// </summary>
        private async Task<string> RenderTree(INodeJSService js, TreeLayoutVM tree, DynamicConfig cfg, CancellationToken token)
        {
            var thoroughness = Interpolator.MapValue(
                cfg.TreeRenderThoroughness,
                new IntervalMap(1, 10, 1, 10),
                new IntervalMap(11, 50, 11, 600),
                new IntervalMap(51, 100, 301, 15000)
            );

            var json = JsonConvert.SerializeObject(tree);
            var result = await js.InvokeFromFileAsync<string>(
                "./External/tree/tree-layout.js",
                args: new object[] {json, thoroughness},
                cancellationToken: token
            );

            if (string.IsNullOrEmpty(result))
                throw new Exception("Failed to render tree: output is empty.");

            return result;
        }

        #endregion

        #region Database processing

        /// <summary>
        /// Removes all existing tree layouts.
        /// </summary>
        private async Task FlushTreeAsync(AppDbContext db)
        {
            using var conn = db.GetConnection();
            await conn.ExecuteAsync(@"
                UPDATE ""Pages"" SET ""TreeLayoutId"" = NULL;
                DELETE FROM ""TreeLayouts""
            ");
        }

        /// <summary>
        /// Updates the layout.
        /// </summary>
        private async Task SaveLayoutAsync(AppDbContext db, TreeLayoutVM tree, TreeLayout layout)
        {
            using var conn = db.GetConnection();
            await conn.ExecuteAsync(
                @"INSERT INTO ""TreeLayouts"" (""Id"", ""LayoutJson"", ""GenerationDate"") VALUES (@Id, @LayoutJson, @GenerationDate)",
                layout
            );

            // sic! dapper generates incorrect query for "GUID IN (@GUIDS)" with parameters
            foreach (var batch in tree.Persons.Select(x => x.Id).PartitionBySize(100))
            {
                var ids = batch.Select(x => $"'{x}'").JoinString(", ");
                await conn.ExecuteAsync($@"UPDATE ""Pages"" SET ""TreeLayoutId"" = '{layout.Id}' WHERE ""Id"" IN ({ids})");
            }
        }

        #endregion
    }
}
