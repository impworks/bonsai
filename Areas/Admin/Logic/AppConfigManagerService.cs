using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Bonsai.Areas.Admin.ViewModels.Config;
using Bonsai.Data;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Admin.Logic
{
    /// <summary>
    /// Service for managing the global app configuration.
    /// </summary>
    public class AppConfigManagerService
    {
        public AppConfigManagerService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        /// <summary>
        /// Gets the update form default values.
        /// </summary>
        public async Task<UpdateAppConfigVM> RequestUpdateAsync()
        {
            return await _db.Config
                            .ProjectTo<UpdateAppConfigVM>()
                            .FirstAsync();
        }

        /// <summary>
        /// Updates the current configuration.
        /// </summary>
        public async Task UpdateAsync(UpdateAppConfigVM request)
        {
            var config = await _db.Config
                                  .FirstAsync();

            _mapper.Map(request, config);
        }
    }
}
