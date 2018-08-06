using System;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Bonsai.Code.Infrastructure;
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

            services.AddAutoMapper(opts =>
            {
                opts.CreateProfile("Default", p =>
                {
                    foreach(var type in types)
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
            });

            Mapper.AssertConfigurationIsValid();
        }
    }
}
