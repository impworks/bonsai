using System;
using System.IO;
using System.Linq;
using Bonsai.Areas.Admin.ViewModels.User;
using Bonsai.Code.Services.Elastic;
using Bonsai.Data.Models;
using Impworks.Utils.Format;
using Microsoft.AspNetCore.Identity;

namespace Bonsai.Data.Utils.Seed
{
    /// <summary>
    /// Collection of seed methods.
    /// </summary>
    public static class SeedData
    {
        /// <summary>
        /// Seeds sample data.
        /// </summary>
        public static void EnsureSeeded(AppDbContext db, ElasticService elastic = null)
        {
            // warning! this REMOVES ALL DATA AND FILES
            ClearPreviousData(db, elastic);

            EnsureSystemItemsSeeded(db);

            var ctx = new SeedContext(db, elastic);

            var root = ctx.AddPage("Иванов Иван Петрович", true, "1990.01.01", descrSource: "SampleDescription.md", factsSource: "SampleHumanFacts.json");
            root.MainPhoto = ctx.AddPhoto("1.jpg");

            // parents
            var dad = ctx.AddPage("Иванов Петр Михайлович", true, "1960.02.03", "2010.03.02");
            var mom = ctx.AddPage("Иванова Екатерина Валерьевна", false, "1965.03.04");

            ctx.AddRelation(root, RelationType.Parent, dad);
            ctx.AddRelation(mom, RelationType.Child, root); // sic! check inverted relations
            ctx.AddRelation(mom, RelationType.Spouse, dad, "1987.02.02-2010.03.02");

            // wife 1
            var w1 = ctx.AddPage("Семенова Анна Николаевна", false, "1992.01.02");
            var c1 = ctx.AddPage("Семенов Евгений Иванович", true, "2012.02.03");
            var c2 = ctx.AddPage("Семенова Екатерина Ивановна", false, "2013.04.01");
            var w1f = ctx.AddPage("Семенов Николай Вадимович", true, "1965.05.06");
            var w1m = ctx.AddPage("Семенова Ирина Алексеевна", false, "1967.06.07");
            var wed = ctx.AddPage("Свадьба Петра Иванова и Анны Семеновой", type: PageType.Event, descrSource: "Свадьба состоялась 5 мая 2010 года.");

            ctx.AddRelation(root, RelationType.Spouse, w1, "2010.05.05-", wed);
            ctx.AddRelations(root, RelationType.Child, c1, c2);
            ctx.AddRelations(w1, RelationType.Child, c1, c2);
            ctx.AddRelations(w1, RelationType.Parent, w1f, w1m);
            ctx.AddRelations(wed, RelationType.EventVisitor, root, w1, mom, dad, w1f, w1m);

            // pet
            var cat = ctx.AddPage("Барсик", true, "2014.05.06", type: PageType.Pet, descrSource: "Пушистый персидский кот!", factsSource: "SamplePetFacts.json");
            ctx.AddRelations(cat, RelationType.Owner, root, w1);

            // events & locations
            var v1 = ctx.AddPage("Отпуск 2015", type: PageType.Event);
            var l1 = ctx.AddPage("Греция", type: PageType.Location);

            ctx.AddRelations(v1, RelationType.EventVisitor, w1, c1, c2);

            // photos
            var pic1 = ctx.AddPhoto("2.jpg", "2015.09.01", explicitId: Guid.Parse("1c40e722-3ce7-49e0-90dd-8c83a6753c0f"));
            ctx.AddMediaTag(pic1, root, MediaTagType.DepictedEntity, "0.35;0.07;0.1;0.15");
            ctx.AddMediaTag(pic1, w1, MediaTagType.DepictedEntity, "0.15;0.12;0.1;0.15");
            ctx.AddMediaTag(pic1, c1, MediaTagType.DepictedEntity, "0.55;0.35;0.1;0.15");
            ctx.AddMediaTag(pic1, c2, MediaTagType.DepictedEntity, "0.35;0.25;0.1;0.15");
            ctx.AddMediaTag(pic1, v1, MediaTagType.Event);
            ctx.AddMediaTag(pic1, l1, MediaTagType.Location);

            var pic2 = ctx.AddPhoto("3.jpg", "2015.09.05");
            ctx.AddMediaTag(pic2, root, MediaTagType.DepictedEntity, "0.2;0.4;0.1;0.4");

            var pic3 = ctx.AddPhoto("3.jpg", null, "Описание для **фотографии** с тегами");
            ctx.AddMediaTag(pic3, root, MediaTagType.DepictedEntity, "0.2;0.4;0.1;0.4");

            var vid1 = ctx.AddVideo("1.mp4", "2015.09.01", "Проверка видео");
            ctx.AddMediaTag(vid1, root);

            ctx.Save();
        }

        /// <summary>
        /// Removes all previous records and files.
        /// </summary>
        private static void ClearPreviousData(AppDbContext db, ElasticService elastic)
        {
            var mediaDir = @".\wwwroot\media";
            if(Directory.Exists(mediaDir))
                foreach(var file in Directory.EnumerateFiles(mediaDir))
                    File.Delete(file);

            db.MediaTags.RemoveRange(db.MediaTags.ToList());
            db.Media.RemoveRange(db.Media.ToList());
            db.Pages.RemoveRange(db.Pages.ToList());
            db.Relations.RemoveRange(db.Relations.ToList());

            db.SaveChanges();

            elastic?.ClearPreviousData();
            elastic?.EnsureIndexesCreated();
        }

        /// <summary>
        /// Adds required records (identity, config, etc.).
        /// </summary>
        private static void EnsureSystemItemsSeeded(AppDbContext db)
        {
            void AddRole(string name) => db.Roles.Add(new IdentityRole {Name = name, NormalizedName = name.ToUpper()});

            if (!db.Roles.Any())
            {
                foreach (var role in EnumHelper.GetEnumValues<UserRole>())
                    AddRole(role.ToString());
            }

            if (!db.Config.Any())
            {
                db.Config.Add(new AppConfig
                {
                    Id = Guid.NewGuid(),
                    Title = "Bonsai",
                    AllowGuests = false
                });
            }

            db.SaveChanges();
        }
    }
}
