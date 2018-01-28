using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
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
        public Page AddPage(string title, bool? gender = null, string birth = null, string death = null, PageType type = PageType.Person, string descrSource = null, string factsSource = null)
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

            if (factsObj["Bio.Gender"] == null && gender != null)
            {
                factsObj["Bio.Gender"] = new JObject{ ["IsMale"] = gender.Value };
            }

            if (birth != null)
            {
                var birthData = factsObj["Birth.Date"] ?? (factsObj["Birth.Date"] = new JObject());
                birthData["Value"] = birth;
            }

            if (death != null)
            {
                var deathData = factsObj["Death.Date"] ?? (factsObj["Death.Date"] = new JObject());
                deathData["Value"] = death;
            }

            var page = new Page
            {
                Id = Guid.NewGuid(),
                Title = title,
                Key = PageHelper.EncodeTitle(title),
                PageType = type,
                Description = (File.Exists(descrFile) ? File.ReadAllText(descrFile) : descrSource) ?? title,
                Facts = factsObj.ToString(Formatting.None),
                CreateDate = DateTimeOffset.Now,
                LastUpdateDate = DateTimeOffset.Now
            };
            _db.Pages.Add(page);
            return page;
        }

        /// <summary>
        /// Creates a new relation between two pages.
        /// </summary>
        public Relation AddRelation(Page source, RelationType type, Page target, string duration = null, bool createComplimentary = true)
        {
            if(!RelationHelper.IsRelationAllowed(source.PageType, target.PageType, type))
                throw new ArgumentException("This relation is not allowed!");

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
        /// Creates a new media file.
        /// </summary>
        public Media AddPhoto(string source, string date = null, string description = null, Guid? explicitId = null)
        {
            var id = explicitId ?? Guid.NewGuid();
            var key = PageHelper.GetMediaKey(id);
            var newName = key + Path.GetExtension(source);
            var sourcePath = @".\Data\Utils\Seed\Media\" + source;
            var diskPath = @".\wwwroot\media\" + newName;
            var webPath = "~/media/" + newName;

            Directory.CreateDirectory(Path.GetDirectoryName(diskPath));

            var paths = EnumHelper.GetEnumValues<MediaSize>()
                                  .Select(x => MediaPresenterService.GetSizedMediaPath(diskPath, x));

            foreach(var path in paths)
                File.Copy(sourcePath, path);

            var media = new Media
            {
                Id = id,
                Type = MediaType.Photo,
                Key = key,
                FilePath = webPath,
                Date = date,
                Description = description,
                Tags = new List<MediaTag>(),
                UploadDate = DateTimeOffset.Now
            };
            _db.Media.Add(media);
            return media;
        }

        /// <summary>
        /// Adds a tag to the media.
        /// </summary>
        public MediaTag AddMediaTag(Media media, Page page, MediaTagType type = MediaTagType.DepictedEntity, string coords = null)
        {
            var ptype = page.PageType;

            if(type == MediaTagType.DepictedEntity)
                if(ptype != PageType.Person && ptype != PageType.Pet && ptype != PageType.Other)
                    throw new ArgumentException("Only Pet, Person and Other types can be marked as DepictedEntity.");

            if(type == MediaTagType.Location && ptype != PageType.Location)
                throw new ArgumentException("Only Location page can be marked as Location.");

            if(type == MediaTagType.Event && ptype != PageType.Event)
                throw new ArgumentException("Only Event page can be marked as Event.");

            var tag = new MediaTag
            {
                Id = Guid.NewGuid(),
                Media = media,
                Object = page,
                Type = type,
                Coordinates = coords
            };
            _db.MediaTags.Add(tag);
            return tag;
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
