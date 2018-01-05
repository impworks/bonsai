using System;
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
        public static void EnsureSeeded(this AppDbContext context)
        {
            if (!context.Pages.Any())
            {
                context.Pages.Add(new Page
                {
                    Id = Guid.NewGuid(),
                    Title = "Иван Иванов",
                    Description = File.ReadAllText(@".\Data\Utils\Seed\SampleDescription.md"),
                    Facts = File.ReadAllText(@".\Data\Utils\Seed\SampleFacts.json")
                });

                context.SaveChanges();
            }
        }
    }
}
