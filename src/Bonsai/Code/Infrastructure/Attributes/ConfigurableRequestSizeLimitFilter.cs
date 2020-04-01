using System;
using Bonsai.Code.Services.Config;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bonsai.Code.Infrastructure.Attributes
{
    /// <summary>
    /// A filter that sets <see cref="IHttpMaxRequestBodySizeFeature.MaxRequestBodySize"/> to a value from the config.
    /// </summary>
    internal class ConfigurableRequestSizeLimitFilter : IAuthorizationFilter, IRequestSizePolicy
    {
        public ConfigurableRequestSizeLimitFilter(BonsaiConfigService cfg)
        {
            _cfg = cfg;
        }

        private readonly BonsaiConfigService _cfg;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var effectivePolicy = context.FindEffectivePolicy<IRequestSizePolicy>();
            if (effectivePolicy != null && effectivePolicy != this)
                return;

            var maxRequestBodySizeFeature = context.HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>();

            if (maxRequestBodySizeFeature?.IsReadOnly != false)
                return;

            maxRequestBodySizeFeature.MaxRequestBodySize = _cfg.GetStaticConfig().WebServer.MaxUploadSize;
        }
    }
}
