using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Search;
using Bonsai.Data.Models;
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
        public static async Task EnsureSampleDataSeededAsync(AppDbContext db, string rootPath = null)
        {
            var ctx = new SeedContext(db, rootPath);

            var root = ctx.AddPage("Иванов Иван Петрович", true, "1990.01.01", descrSource: "SampleDescription.md", factsSource: "SampleHumanFacts.json");
            root.MainPhoto = ctx.AddPhoto("1.jpg");

            // parents
            var dad = ctx.AddPage("Иванов Петр Михайлович", true, "1960.02.03", "2010.03.02");
            var mom = ctx.AddPage("Иванова Екатерина Валерьевна", false, "1965.03.04");
            var stepdad = ctx.AddPage("Михайлов Олег Евгеньевич", true, "1962.04.03");

            ctx.AddRelation(root, RelationType.Parent, dad);
            ctx.AddRelation(mom, RelationType.Child, root); // sic! check inverted relations
            ctx.AddRelation(mom, RelationType.Spouse, dad, "1987.02.02-2000.03.02");
            ctx.AddRelation(mom, RelationType.Spouse, stepdad, "2000.04.02-2010.03.02");

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

            await ctx.SaveAsync();
        }

        /// <summary>
        /// Adds an administrator user account.
        /// </summary>
        public static async Task EnsureDefaultUserCreatedAsync(UserManager<AppUser> userMgr)
        {
            await AddUser("Тестовый Админ", "admin@example.com", "123456");

            async Task AddUser(string name, string email, string password, UserRole role = UserRole.Admin)
            {
                var parts = name.Split(' ');
                var user = new AppUser
                {
                    IsValidated = true,
                    AuthType = AuthType.Password,
                    UserName = email,
                    Email = email,
                    LastName = parts.Length > 0 ? parts[0] : "",
                    FirstName = parts.Length > 1 ? parts[1] : "",
                    MiddleName = parts.Length > 2 ? parts[2] : ""
                };

                var result = await userMgr.CreateAsync(user);
                if (!result.Succeeded)
                    return;

                await userMgr.AddPasswordAsync(user, password);
                await userMgr.AddToRoleAsync(user, role.ToString());
            }
        }

        /// <summary>
        /// Removes all previous records and files.
        /// </summary>
        public static async Task ClearPreviousDataAsync(AppDbContext db)
        {
            var mediaDir = @".\wwwroot\media";
            if (Directory.Exists(mediaDir))
                foreach (var file in Directory.EnumerateFiles(mediaDir))
                    File.Delete(file);

            db.Changes.RemoveRange(db.Changes.ToList());
            db.MediaTags.RemoveRange(db.MediaTags.ToList());
            db.Media.RemoveRange(db.Media.ToList());
            db.Relations.RemoveRange(db.Relations.ToList());
            db.Pages.RemoveRange(db.Pages.ToList().Except(db.Users.Select(x => x.Page).Where(x => x != null).ToList()));

            await db.SaveChangesAsync();
        }
    }
}
