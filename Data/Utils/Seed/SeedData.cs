using System.IO;
using System.Linq;
using Bonsai.Data.Models;

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
        public static void EnsureSeeded(this AppDbContext db)
        {
            // warning! this REMOVES ALL DATA AND FILES
            ClearPreviousData(db);

            var ctx = new SeedContext(db);

            var root = ctx.AddPage("Иванов Иван Петрович", true, "1990.01.01", descrSource: "SampleDescription.md", factsSource: "SampleHumanFacts.json");
            root.MainPhoto = ctx.AddPhoto("1.jpg");
            var pic1 = ctx.AddPhoto("2.jpg", "2015.09.01");
            var pic2 = ctx.AddPhoto("3.jpg", "2015.09.05");
            ctx.AddMediaTag(pic1, root, MediaTagType.DepictedEntity, "0.1;0.3;0.1;0.3");
            ctx.AddMediaTag(pic2, root, MediaTagType.DepictedEntity, "0.2;0.4;0.1;0.4");

            // parents
            var dad = ctx.AddPage("Иванов Петр Михайлович", true, "1960.02.03", "2010.03.02");
            var mom = ctx.AddPage("Иванова Екатерина Валерьевна", false, "1965.03.04");

            ctx.AddRelation(root, RelationType.Parent, dad);
            ctx.AddRelation(mom, RelationType.Child, root);
            ctx.AddRelation(mom, RelationType.Spouse, dad, "1987.02.02-2010.03.02");

            // wife 1
            var w1 = ctx.AddPage("Семенова Анна Николаевна", false, "1992.01.02");
            var c1 = ctx.AddPage("Семенов Евгений Иванович", true, "2012.02.03");
            var c2 = ctx.AddPage("Семенова Екатерина Ивановна", false, "2013.04.01");
            var w1f = ctx.AddPage("Семенов Николай Вадимович", true, "1965.05.06");
            var w1m = ctx.AddPage("Семенова Ирина Алексеевна", false, "1967.06.07");

            ctx.AddRelation(root, RelationType.Spouse, w1, "2010.05.05-");
            ctx.AddRelation(root, RelationType.Child, c1);
            ctx.AddRelation(root, RelationType.Child, c2);
            ctx.AddRelation(w1, RelationType.Child, c1);
            ctx.AddRelation(w1, RelationType.Child, c2);
            ctx.AddRelation(w1, RelationType.Parent, w1f);
            ctx.AddRelation(w1, RelationType.Parent, w1m);

            // pet
            var cat = ctx.AddPage("Барсик", true, "2014.05.06", type: PageType.Pet, descrSource: "Пушистый персидский кот!", factsSource: "SamplePetFacts.json");
            ctx.AddRelation(cat, RelationType.Owner, root);
            ctx.AddRelation(cat, RelationType.Owner, w1);

            ctx.Save();
        }

        /// <summary>
        /// Removes all previous records and files.
        /// </summary>
        private static void ClearPreviousData(AppDbContext db)
        {
            foreach(var file in Directory.EnumerateFiles(@".\wwwroot\media"))
                File.Delete(file);

            db.MediaTags.RemoveRange(db.MediaTags.ToList());
            db.Media.RemoveRange(db.Media.ToList());
            db.Pages.RemoveRange(db.Pages.ToList());
            db.Relations.RemoveRange(db.Relations.ToList());

            db.SaveChanges();
        }
    }
}
