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
            if (db.Pages.Any())
                return;

            var ctx = new SeedContext(db);

            // parents
            var root = ctx.AddPage("Иван Иванов", true, "1990.01.01", descrSource: "SampleDescription.md", factsSource: "SampleFacts.json");
            var dad = ctx.AddPage("Петр Иванов", true, "1960.02.03", "2010.03.02");
            var mom = ctx.AddPage("Екатерина Иванова", false, "1965.03.04");

            ctx.AddRelation(root, RelationType.Parent, dad);
            ctx.AddRelation(mom, RelationType.Child, root);
            ctx.AddRelation(mom, RelationType.Spouse, dad, "1987.02.02-2010.03.02");

            // wife 1
            var w1 = ctx.AddPage("Анна Семенова", false, "1992.01.02");
            var c1 = ctx.AddPage("Евгений Семенов", true, "2012.02.03");
            var c2 = ctx.AddPage("Екатерина Семенова", false, "2013.04.01");
            var w1f = ctx.AddPage("Николай Семенов", true, "1965.05.06");
            var w1m = ctx.AddPage("Ирина Семенова", false, "1967.06.07");

            ctx.AddRelation(root, RelationType.Spouse, w1, "2010.05.05");
            ctx.AddRelation(root, RelationType.Child, c1);
            ctx.AddRelation(root, RelationType.Child, c2);
            ctx.AddRelation(w1, RelationType.Child, c1);
            ctx.AddRelation(w1, RelationType.Child, c2);
            ctx.AddRelation(w1, RelationType.Parent, w1f);
            ctx.AddRelation(w1, RelationType.Parent, w1m);

            // pet
            var cat = ctx.AddPage("Барсик", true, "2014.05.06", type: PageType.Pet, descrSource: "Персидский кот");
            ctx.AddRelation(cat, RelationType.Owner, root);
            ctx.AddRelation(cat, RelationType.Owner, w1);

            db.SaveChanges();
        }
    }
}
