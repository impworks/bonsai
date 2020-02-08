using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Bonsai.Code.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Config
{
    public partial class Startup
    {
        /// <summary>
        /// Registers Automapper.
        /// </summary>
        private void ConfigureAutomapper(IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly()
                                .GetTypes()
                                .Where(x => x.IsClass
                                            && !x.IsAbstract
                                            && x.GetInterfaces().Any(y => y == typeof(IMapped))
                                            && x.GetMethods().Any(y => y.Name == nameof(IMapped.Configure) && y.DeclaringType == x))
                                .ToList();

            void CreateProfile(IMapperConfigurationExpression opts)
            {
                opts.CreateProfile("Default", p =>
                {
                    foreach (var type in types)
                    {
                        try
                        {
                            var t = (IMapped)Activator.CreateInstance(type);
                            t.Configure(p);
                        }
                        catch
                        {
                            // do nothing
                        }
                    }
                });
            }

            services.AddSingleton<IConfigurationProvider>(sp => new MapperConfiguration(CreateProfile));
            services.AddScoped<IMapper>(sp => new Mapper(sp.GetRequiredService<IConfigurationProvider>(), sp.GetService));
        }

        /// <summary>
        /// Ensures that the mapper configuration is valid.
        /// </summary>
        private void ValidateAutomapperConfig(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var mapper = scope.ServiceProvider.GetService<IMapper>();
                mapper.ConfigurationProvider.AssertConfigurationIsValid();
            }
        }
    }
}
