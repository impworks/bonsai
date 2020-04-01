using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Code.Infrastructure.Attributes
{
    /// <summary>
    /// Sets the upload size limit from the static configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ConfigurableRequestSizeLimitAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public int Order { get; } = 900;
        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ConfigurableRequestSizeLimitFilter>();
    }
}
