using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Dapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Code.DomainModel.Relations
{
    /// <summary>
    /// Information about all known pages and relations.
    /// </summary>
    public class RelationContext
    {
        #region Properties

        /// <summary>
        /// List of all pages currently available.
        /// </summary>
        public IReadOnlyDictionary<Guid, PageExcerpt> Pages { get; private set; }

        /// <summary>
        /// List of all relations currently available.
        /// </summary>
        public IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>> Relations { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds extra information about a page.
        /// </summary>
        public void Augment(PageExcerpt page)
        {
            var pages = (Dictionary<Guid, PageExcerpt>) Pages;
            pages[page.Id] = page;
        }

        /// <summary>
        /// Adds extra information about a relation.
        /// </summary>
        public void Augment(RelationExcerpt rel)
        {
            var rels = (Dictionary<Guid, IReadOnlyList<RelationExcerpt>>) Relations;
            if (rels.TryGetValue(rel.SourceId, out var rList))
            {
                var list = (List<RelationExcerpt>) rList;
                var existing = list.FirstOrDefault(x => x.DestinationId == rel.DestinationId && x.Type == rel.Type);
                if (existing != null)
                    list.Remove(existing);

                list.Add(rel);
            }
            else
            {
                rels[rel.SourceId] = new List<RelationExcerpt> {rel};
            }
        }

        #endregion

        #region Static constructor

        /// <summary>
        /// Loads basic information about all pages.
        /// </summary>
        public static async Task<RelationContext> LoadContextAsync(AppDbContext db, RelationContextOptions opts = null)
        {
            if(opts == null)
                opts = new RelationContextOptions();

            var pages = await LoadPagesAsync(db, opts);
            var rels = await LoadRelationsAsync(db, opts);

            return new RelationContext
            {
                Pages = pages.ToDictionary(x => x.Id, x => x),
                Relations = rels
            };
        }

        /// <summary>
        /// Loads the pages from the database.
        /// </summary>
        private static async Task<List<PageExcerpt>> LoadPagesAsync(AppDbContext db, RelationContextOptions opts)
        {
            var filterByPeople = opts.PeopleOnly
                ? @"AND p.""Type"" = 0"
                : "";

            using (var conn = db.GetConnection())
            {
                var pagesSource = await conn.QueryAsync<PageExcerpt>(@"
                    SELECT
                        t.""Id"",
                        t.""Title"",
                        t.""Key"",
                        t.""Type"",
                        t.""BirthDate"",
                        t.""DeathDate"",
                        t.""IsDead"",
                        t.""Gender"",
                        COALESCE(
                            t.""Nickname"",
                            CASE
                                WHEN t.""LastName"" IS NULL THEN NULL
                                ELSE CONCAT(t.""FirstName"", ' ', t.""LastName"")
                            END
                        ) AS ""ShortName"",
                        CASE
                            WHEN t.""MaidenName"" = t.""LastName"" THEN NULL
                            ELSE t.""MaidenName""
                        END AS ""MaidenName"",
                        t.""MainPhotoPath""
                    FROM (
                        SELECT
                            p.""Id"",
                            p.""Title"",
                            p.""Key"",
                            p.""Type"",
                            p.""Facts""::json#>>'{Main.Name,Values,-1,FirstName}' AS ""FirstName"",
                            p.""Facts""::json#>>'{Main.Name,Values,-1,LastName}' AS ""LastName"",
                            p.""Facts""::json#>>'{Main.Name,Value}' AS ""Nickname"",
                            p.""Facts""::json#>>'{Main.Name,Values,0,LastName}' AS ""MaidenName"",
                            p.""Facts""::json#>>'{Birth.Date,Value}' AS ""BirthDate"",
                            p.""Facts""::json#>>'{Death.Date,Value}' AS ""DeathDate"",
                            p.""Facts""::json->'Death.Date' IS NOT NULL AS ""IsDead"",
                            CAST(p.""Facts""::json#>>'{Bio.Gender,IsMale}' AS BOOLEAN) AS ""Gender"",
                            m.""FilePath"" AS ""MainPhotoPath""
                        FROM ""Pages"" AS p
                        LEFT JOIN ""Media"" AS m ON m.""Id"" = p.""MainPhotoId"" AND m.""IsDeleted"" = false
                        WHERE p.""IsDeleted"" = false " + filterByPeople + @"
                    ) AS t
                ");

                return pagesSource.ToList();
            }
        }

        /// <summary>
        /// Loads the relations from the database.
        /// </summary>
        private static async Task<IReadOnlyDictionary<Guid, IReadOnlyList<RelationExcerpt>>> LoadRelationsAsync(AppDbContext db, RelationContextOptions opts)
        {
            if (opts.PagesOnly)
                return null;

            var query =  db.Relations
                           .Where(x => x.Source.IsDeleted == false
                                       && x.Destination.IsDeleted == false
                                       && x.IsDeleted == false);
            if (opts.PeopleOnly)
            {
                query = query.Where(x => x.Source.Type == PageType.Person
                                         && x.Destination.Type == PageType.Person);
            }

            var data = await query.Select(x => new RelationExcerpt
                              {
                                  Id = x.Id,
                                  SourceId = x.SourceId,
                                  DestinationId = x.DestinationId,
                                  EventId = x.Event != null && x.Event.IsDeleted == false ? x.EventId : null,
                                  Duration = FuzzyRange.TryParse(x.Duration),
                                  Type = x.Type,
                                  IsComplementary = x.IsComplementary
                              })
                              .ToListAsync();

            return data.GroupBy(x => x.SourceId).ToDictionary(x => x.Key, x => (IReadOnlyList<RelationExcerpt>) x.ToList());
        }

        #endregion

        #region Nested classes

        /// <summary>
        /// Basic information about a page.
        /// </summary>
        [DebuggerDisplay("{Title} ({Id})")]
        public class PageExcerpt : IEquatable<PageExcerpt>
        {
            public Guid Id { get; set; }

            public string Title { get; set; }
            public string Key { get; set; }
            public PageType Type { get; set; }
            public bool? Gender { get; set; }
            public FuzzyDate? BirthDate { get; set; }
            public FuzzyDate? DeathDate { get; set; }
            public bool IsDead { get; set; }
            public string ShortName { get; set; }
            public string MaidenName { get; set; }
            public string MainPhotoPath { get; set; }

            #region Equality members (auto-generated)

            public bool Equals(PageExcerpt other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PageExcerpt)obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }

            public static bool operator ==(PageExcerpt left, PageExcerpt right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(PageExcerpt left, PageExcerpt right)
            {
                return !Equals(left, right);
            }

            #endregion
        }

        /// <summary>
        /// Basic information about a relation between two pages.
        /// </summary>
        [DebuggerDisplay("{Type}: {SourceId} -> {DestinationId} ({Duration})")]
        public class RelationExcerpt
        {
            public Guid Id { get; set; }

            public Guid SourceId { get; set; }
            public Guid DestinationId { get; set; }
            public Guid? EventId { get; set; }
            public RelationType Type { get; set; }
            public FuzzyRange? Duration { get; set; }
            public bool IsComplementary { get; set; }
        }

        #endregion
    }
}
