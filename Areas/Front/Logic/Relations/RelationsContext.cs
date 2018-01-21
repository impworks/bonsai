using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Tools;
using Bonsai.Data;
using Bonsai.Data.Models;
using Bonsai.Data.Utils;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic.Relations
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

        #region Static constructor

        /// <summary>
        /// Loads basic information about all pages.
        /// </summary>
        public static async Task<RelationContext> LoadContextAsync(AppDbContext db)
        {
            using (var conn = db.GetConnection())
            {
                var pagesSource = await conn.QueryAsync<PageExcerpt>(@"
                                    SELECT
                                        t.""Id"",
                                        t.""Title"",
                                        t.""Key"",
                                        t.""PageType"",
                                        t.""BirthDate"",
                                        t.""DeathDate"",
                                        t.""Gender"",
                                        COALESCE(t.""Nickname"", CONCAT(t.""FirstName"", ' ', t.""LastName"")) AS ""ShortName"",
                                        t.""MainPhotoPath""
                                    FROM (
                                        SELECT
                                            p.""Id"",
                                            p.""Title"",
                                            p.""Key"",
                                            p.""PageType"",
                                            p.""Facts""::json#>>'{Main.Name,Values,-1,FirstName}' AS ""FirstName"",
                                            p.""Facts""::json#>>'{Main.Name,Values,-1,LastName}' AS ""LastName"",
                                            p.""Facts""::json#>>'{Main.Name,Value}' AS ""Nickname"",
                                            p.""Facts""::json#>>'{Birth.Date,Value}' AS ""BirthDate"",
                                            p.""Facts""::json#>>'{Death.Date,Value}' AS ""DeathDate"",
                                            CAST(p.""Facts""::json#>>'{Bio.Gender,IsMale}' AS BOOLEAN) AS ""Gender"",
                                            m.FilePath AS ""MainPhotoPath""
                                        FROM ""Pages"" AS p
                                        LEFT JOIN ""Media"" AS m ON m.Id = p.MainPhotoId
                                    ) AS t
                                  ").ConfigureAwait(false);

                var pages = pagesSource.ToList();
                foreach (var page in pages)
                    page.MainPhotoPath = MediaPresenterService.GetSizedMediaPath(page.MainPhotoPath, MediaSize.Small);

                var relations = await db.Relations
                                         .Select(x => new RelationExcerpt
                                         {
                                             SourceId = x.SourceId,
                                             DestinationId = x.DestinationId,
                                             Duration = FuzzyRange.TryParse(x.Duration),
                                             Type = x.Type
                                         })
                                         .GroupBy(x => x.SourceId)
                                         .ToDictionaryAsync(x => x.Key, x => (IReadOnlyList<RelationExcerpt>)x.ToList())
                                         .ConfigureAwait(false);

                return new RelationContext
                {
                    Pages = pages.ToDictionary(x => x.Id, x => x),
                    Relations = relations
                };
            }
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
            public PageType PageType { get; set; }
            public bool? Gender { get; set; }
            public FuzzyDate? BirthDate { get; set; }
            public FuzzyDate? DeathDate { get; set; }
            public string ShortName { get; set; }
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
            public Guid SourceId { get; set; }
            public Guid DestinationId { get; set; }
            public RelationType Type { get; set; }
            public FuzzyRange? Duration { get; set; }
        }

        #endregion
    }
}
