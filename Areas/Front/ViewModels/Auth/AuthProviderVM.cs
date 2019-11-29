using System;
using Bonsai.Code.Services.Config;
using Microsoft.AspNetCore.Authentication;

namespace Bonsai.Areas.Front.ViewModels.Auth
{
    /// <summary>
    /// Information about an authorization provider.
    /// </summary>
    public class AuthProviderVM
    {
        /// <summary>
        /// Internal name of the provider.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// CSS name of the class for the button.
        /// </summary>
        public string IconClass { get; set; }

        /// <summary>
        /// Readable name of the caption.
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// The handler for activating current provider.
        /// </summary>
        public Func<StaticConfig, AuthenticationBuilder, bool> TryActivate { get; set; }
    }
}
