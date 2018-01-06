using System;
using System.IO;
using System.Linq;
using Bonsai.Code.Utils;
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
                var pageId = Guid.NewGuid();
                var pageTitle = "Иван Иванов";

                context.Pages.Add(new Page
                {
                    Id = pageId,
                    Title = pageTitle,
                    Key = PageHelper.EncodeTitle(pageTitle),
                    Description = File.ReadAllText(@".\Data\Utils\Seed\SampleDescription.md"),
                    Facts = File.ReadAllText(@".\Data\Utils\Seed\SampleFacts.json")
                });

                context.SaveChanges();
            }
        }
    }
}
