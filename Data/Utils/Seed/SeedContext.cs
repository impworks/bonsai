using System;
using System.IO;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;
using Bonsai.Data.Models;

namespace Bonsai.Data.Utils.Seed
{
    /// <summary>
    /// A nice wrapper for seeding data.
    /// </summary>
    public class SeedContext
    {
        public SeedContext(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Creates a new page.
        /// </summary>
        public Page AddPage(string title, bool? gender, string birth = null, string death = null, PageType type = PageType.Person, string descrSource = null, string factsSource = null)
        {
            var descrFile = @".\Data\Utils\Seed\" + descrSource;
            var factsFile = @".\Data\Utils\Seed\" + factsSource;

            var page = new Page
            {
                Id = Guid.NewGuid(),
                Title = title,
                Key = PageHelper.EncodeTitle(title),
                PageType = type,
                Gender = gender,
                BirthDate = birth,
                DeathDate = death,
                Description = (File.Exists(descrFile) ? File.ReadAllText(descrFile) : descrSource) ?? title,
                Facts = (File.Exists(factsFile) ? File.ReadAllText(factsFile) : factsSource) ?? "{}",
            };
            _db.Pages.Add(page);
            return page;
        }

        /// <summary>
        /// Creates a new relation between two pages.
        /// </summary>
        public Relation AddRelation(Page source, RelationType type, Page target, string duration = null, bool createComplimentary = true)
        {
            var rel = new Relation
            {
                Id = Guid.NewGuid(),
                Source = source,
                Destination = target,
                Type = type,
                Duration = duration,
                IsComplementary = false
            };

            _db.Relations.Add(rel);

            if (createComplimentary)
            {
                var invRel = new Relation
                {
                    Id = Guid.NewGuid(),
                    Source = target,
                    Destination = source,
                    Type = RelationHelper.ComplementaryRelations[type],
                    Duration = duration,
                    IsComplementary = true
                };
                _db.Relations.Add(invRel);
            }

            return rel;
        }
    }
}
