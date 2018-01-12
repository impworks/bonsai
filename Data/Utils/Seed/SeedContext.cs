using System;
using System.IO;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils;
using Bonsai.Data.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

            var factsObj = JObject.Parse(File.Exists(factsFile) ? File.ReadAllText(factsFile) : factsSource ?? "{}");

            if (factsObj["Main.Name"] == null)
            {
                if (type == PageType.Person)
                {
                    var titleParts = title.Split(' ');
                    var nameData = new JObject {["Range"] = $"{birth}-{death}"};
                    if (titleParts.Length > 0) nameData["LastName"] = titleParts[0];
                    if (titleParts.Length > 1) nameData["FirstName"] = titleParts[1];
                    if (titleParts.Length > 2) nameData["MiddleName"] = titleParts[2];

                    factsObj["Main.Name"] = new JObject
                    {
                        ["Values"] = new JArray {nameData}
                    };
                }
                else
                {
                    factsObj["Main.Name"] = new JObject { ["Value"] = title };
                }
            }

            if (birth != null)
            {
                var birthData = factsObj["Birth"] ?? (factsObj["Birth"] = new JObject());
                birthData["Date"] = birth;
            }

            if (death != null)
            {
                var deathData = factsObj["Death"] ?? (factsObj["Death"] = new JObject());
                deathData["Date"] = death;
            }

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
                Facts = factsObj.ToString(Formatting.None),
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

        /// <summary>
        /// Commits the entities to the database.
        /// </summary>
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
