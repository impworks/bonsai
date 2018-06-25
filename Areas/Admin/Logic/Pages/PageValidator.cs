using System.Threading.Tasks;
using Bonsai.Data;
using Bonsai.Data.Models;

namespace Bonsai.Areas.Admin.Logic.Pages
{
    /// <summary>
    /// The validator that checks relation/fact consistency.
    /// </summary>
    public class PageValidator
    {
        public PageValidator(AppDbContext db)
        {
            _db = db;
        }

        private readonly AppDbContext _db;

        /// <summary>
        /// Checks the current relations and prepares them for database serialization.
        /// </summary>
        public async Task<PageValidationResult> ValidateAsync(Page page, string relations, string facts)
        {
            // todo
            return new PageValidationResult();
        }
    }
}
