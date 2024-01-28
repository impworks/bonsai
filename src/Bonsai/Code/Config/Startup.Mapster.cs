using Bonsai.Code.Infrastructure;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Registers Mapster.
        /// </summary>
        private void ConfigureMapster(IServiceCollection services)
        {
            var config = new TypeAdapterConfig
            {
                RequireExplicitMapping = true,
            };

            foreach (var map in RuntimeHelper.GetAllInstances<IMapped>())
                map.Configure(config);

            config.Compile();
            var mapper = new Mapper(config);

            services.AddSingleton<IMapper>(mapper);
        }
    }
}
