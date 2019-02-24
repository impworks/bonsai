using System.Collections.Generic;
using Bonsai.Areas.Front.ViewModels.Auth;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// The service for handling external authentication providers.
    /// </summary>
    public class AuthProviderService
    {
        /// <summary>
        /// The total list of supported providers.
        /// </summary>
        private static List<AuthProviderVM> SupportedProviders = new List<AuthProviderVM>
        {
            new AuthProviderVM
            {
                Key = "Facebook",
                Caption = "Facebook",
                IconClass = "fa fa-facebook-square",
                TryActivate = (cfg, auth) =>
                {
                    var id = cfg["Auth:Facebook:AppId"];
                    var secret = cfg["Auth:Facebook:AppSecret"];
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                        return false;

                    auth.AddFacebook(opts =>
                    {
                        opts.AppId = id;
                        opts.AppSecret = secret;
                        opts.Scope.Add("email");
                    });

                    return true;
                }
            },

            new AuthProviderVM
            {
                Key = "Google",
                Caption = "Google",
                IconClass = "fa fa-google-plus-square",
                TryActivate = (cfg, auth) =>
                {
                    var id = cfg["Auth:Google:ClientId"];
                    var secret = cfg["Auth:Google:ClientSecret"];
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                        return false;

                    auth.AddGoogle(opts =>
                    {
                        opts.ClientId = id;
                        opts.ClientSecret = secret;
                        opts.Scope.AddRange(new [] { "email", "profile" });
                    });

                    return true;
                }
            },

            new AuthProviderVM
            {
                Key = "VK",
                Caption = "Вконтакте",
                IconClass = "fa fa-google-plus-square",
                TryActivate = (cfg, auth) =>
                {
                    var id = cfg["Auth:Google:ClientId"];
                    var secret = cfg["Auth:Google:ClientSecret"];
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                        return false;

                    auth.AddGoogle(opts =>
                    {
                        opts.ClientId = id;
                        opts.ClientSecret = secret;
                        opts.Scope.AddRange(new [] { "email", "profile" });
                    });

                    return true;
                }
            },
        };

        /// <summary>
        /// List of providers that are currently configured as active.
        /// </summary>
        public IReadOnlyList<AuthProviderVM> AvailableProviders { get; private set; }

        /// <summary>
        /// Configures all enabled configuration providers.
        /// </summary>
        public void Initialize(IConfiguration config, AuthenticationBuilder authBuilder)
        {
            var available = new List<AuthProviderVM>();

            foreach (var prov in SupportedProviders)
                if(prov.TryActivate(config, authBuilder))
                    available.Add(prov);

            AvailableProviders = available;
        }
    }
}
