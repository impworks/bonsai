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

            var root = ctx.AddPage("Иван Иванов", true, "1990.01.01", descrSource: "SampleDescription.md", factsSource: "SampleFacts.json");
            var dad = ctx.AddPage("Петр Иванов", true, "1960.02.03", "2010.03.02");
            var mom = ctx.AddPage("Екатерина Иванова", false, "1965.03.04");

            ctx.AddRelation(root, RelationType.Parent, dad);
            ctx.AddRelation(mom, RelationType.Child, root);
            ctx.AddRelation(mom, RelationType.Spouse, dad, "1987.02.02-2010.03.02");

            db.SaveChanges();
        }
    }
}
