using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.DomainModel.Relations;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Impworks.Utils.Strings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bonsai.Data.Utils.Seed
{
    /// <summary>
    /// A nice wrapper for seeding data.
    /// </summary>
    public class SeedContext
    {
        public SeedContext(AppDbContext db, string rootPath = null)
        {
            _db = db;
            _rootPath = StringHelper.Coalesce(rootPath, Path.Combine(Path.GetFullPath(Directory.GetCurrentDirectory()), "Data", "Utils", "Seed"));
        }

        private readonly AppDbContext _db;
        private readonly string _rootPath;

        /// <summary>
        /// Creates a new page.
        /// </summary>
        public Page AddPage(string title, bool? gender = null, string birth = null, string death = null, PageType type = PageType.Person, string descrSource = null, string factsSource = null)
        {
            var descrFile = Path.Combine(_rootPath, descrSource ?? "");
            var factsFile = Path.Combine(_rootPath, factsSource ?? "");

            var factsObj = JObject.Parse(
                File.Exists(factsFile)
                    ? File.ReadAllText(factsFile)
                    : factsSource ?? "{}"
            );

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

            var key = PageHelper.EncodeTitle(title);
            var page = new Page
            {
                Id = Guid.NewGuid(),
                Title = title,
                Key = key,
                Type = type,
                Description = (File.Exists(descrFile) ? File.ReadAllText(descrFile) : descrSource) ?? title,
                Facts = factsObj.ToString(Formatting.None),
                CreationDate = DateTimeOffset.Now,
                LastUpdateDate = DateTimeOffset.Now
            };
            _db.Pages.Add(page);
            _db.PageAliases.Add(new PageAlias {Id = Guid.NewGuid(), Key = key.ToLowerInvariant(), Title = title, Page = page});

            return page;
        }

        /// <summary>
        /// Creates a new relation between two pages.
        /// </summary>
        public Relation AddRelation(Page source, RelationType type, Page target, string duration = null, Page eventPage = null)
        {
            if(!RelationHelper.IsRelationAllowed(source.Type, target.Type, type))
                throw new ArgumentException("This relation is not allowed!");

            if (eventPage != null)
            {
                if(!RelationHelper.IsRelationEventReferenceAllowed(type))
                    throw new ArgumentException("This relation cannot have an event reference.");

                if(eventPage.Type != PageType.Event)
                    throw new ArgumentException("The related event page must have Event type.");
            }

            var rel = new Relation
            {
                Id = Guid.NewGuid(),
                Source = source,
                Destination = target,
                Event = eventPage,
                Type = type,
                Duration = duration,
                IsComplementary = false
            };
            _db.Relations.Add(rel);

            var invRel = new Relation
            {
                Id = Guid.NewGuid(),
                Source = target,
                Destination = source,
                Event = eventPage,
                Type = RelationHelper.ComplementaryRelations[type],
                Duration = duration,
                IsComplementary = true
            };
            _db.Relations.Add(invRel);

            return rel;
        }

        /// <summary>
        /// Adds a bulk of relations.
        /// </summary>
        public void AddRelations(Page source, RelationType type, params Page[] pages)
        {
            if(pages.Length == 0)
                throw new ArgumentException("At least one target must be specified.");

            foreach (var rel in pages)
                AddRelation(source, type, rel);
        }

        /// <summary>
        /// Creates a photo media record.
        /// </summary>
        public Media AddPhoto(string source, string date = null, string description = null, Guid? explicitId = null)
        {
            return AddMedia(
                MediaType.Photo,
                source,
                source,
                date,
                description,
                explicitId
            );
        }

        /// <summary>
        /// Creates a video media record.
        /// </summary>
        public Media AddVideo(string source, string date = null, string description = null, Guid? explicitId = null)
        {
            return AddMedia(
                MediaType.Video,
                source,
                "video-preview.png",
                date,
                description,
                explicitId
            );
        }

        /// <summary>
        /// Creates a document media record.
        /// </summary>
        public Media AddDocument(string source, string date = null, string description = null, Guid? explicitId = null)
        {
            return AddMedia(
                MediaType.Document,
                source,
                "doc-preview.png",
                date,
                description,
                explicitId
            );
        }

        /// <summary>
        /// Adds a tag to the media.
        /// </summary>
        public MediaTag AddMediaTag(Media media, Page page, MediaTagType type = MediaTagType.DepictedEntity, string coords = null)
        {
            var ptype = page.Type;

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
        public async Task SaveAsync()
        {
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a generic media element.
        /// </summary>
        private Media AddMedia(MediaType type, string source, string preview = null, string date = null, string description = null, Guid? explicitId = null, string mimeType = null)
        {
            string GetDefaultMimeType()
            {
                if (type == MediaType.Photo)
                    return "image/jpeg";

                if (type == MediaType.Document)
                    return "application/pdf";

                if (type == MediaType.Video)
                    return "video/mpeg";

                throw new ArgumentException("Unknown MediaType!");
            }

            var id = explicitId ?? Guid.NewGuid();
            var key = PageHelper.GetMediaKey(id);
            var newName = key + Path.GetExtension(source);
            var sourcePath = Path.Combine(_rootPath, "Media", source);
            var diskPath = Path.Combine(Path.GetFullPath(Directory.GetCurrentDirectory()), "wwwroot", "media", newName);
            var webPath = "~/media/" + newName;

            Directory.CreateDirectory(Path.GetDirectoryName(diskPath));

            // copy original file
            File.Copy(sourcePath, diskPath);

            // copy previews
            var paths = EnumHelper.GetEnumValues<MediaSize>()
                                  .Where(x => x != MediaSize.Original)
                                  .Select(x => MediaPresenterService.GetSizedMediaPath(diskPath, x));

            foreach(var path in paths)
                File.Copy(Path.Combine(_rootPath, "Media", preview), path);

            var media = new Media
            {
                Id = id,
                Type = type,
                Key = key,
                FilePath = webPath,
                Date = date,
                Description = description,
                Tags = new List<MediaTag>(),
                UploadDate = DateTimeOffset.Now,
                IsProcessed = true,
                MimeType = mimeType ?? GetDefaultMimeType()
            };
            _db.Media.Add(media);
            return media;
        }
    }
}
