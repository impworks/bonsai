using System.Collections.Generic;
using System.Security.Claims;
using Bonsai.Areas.Front.ViewModels.Auth;
using Bonsai.Code.Services.Config;
using Impworks.Utils.Linq;
using Microsoft.AspNetCore.Authentication;
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
                Key = "Vkontakte",
                Caption = "ВКонтакте",
                IconClass = "fa fa-vk",
                TryActivate = (cfg, auth) =>
                {
                    if (cfg?.Auth?.Vkontakte == null)
                        return false;

                    var id = cfg.Auth.Vkontakte?.ClientId;
                    var secret = cfg.Auth.Vkontakte?.ClientSecret;
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                        return false;

                    auth.AddVkontakte(opts =>
                    {
                        opts.ClientId = id;
                        opts.ClientSecret = secret;
                        opts.Scope.AddRange(new [] { "email" });
                        opts.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "bdate");
                        opts.Fields.Add("bdate");
                        opts.AccessDeniedPath = "/auth/failed";
                    });

                    return true;
                }
            },

            new AuthProviderVM
            {
                Key = "Yandex",
                Caption = "Яндекс",
                IconClass = "fa fa-yahoo", // the closest one that has an Y :)
                TryActivate = (cfg, auth) =>
                {
                    if (cfg?.Auth?.Yandex == null)
                        return false;

                    var id = cfg.Auth.Yandex?.ClientId;
                    var secret = cfg.Auth.Yandex?.ClientSecret;
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                        return false;

                    auth.AddYandex(opts =>
                    {
                        opts.ClientId = id;
                        opts.ClientSecret = secret;
                        opts.Scope.AddRange(new [] { "login:email", "login:birthday", "login:info" });
                        opts.ClaimActions.MapJsonKey(ClaimTypes.DateOfBirth, "birthday");
                        opts.AccessDeniedPath = "/auth/failed";
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
                    if (cfg?.Auth?.Google == null)
                        return false;

                    var id = cfg.Auth.Google?.ClientId;
                    var secret = cfg.Auth.Google?.ClientSecret;
                    if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
                        return false;

                    auth.AddGoogle(opts =>
                    {
                        opts.ClientId = id;
                        opts.ClientSecret = secret;
                        opts.Scope.AddRange(new [] { "email", "profile" });
                        opts.AccessDeniedPath = "/auth/failed";
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
        public void Initialize(StaticConfig config, AuthenticationBuilder authBuilder)
        {
            var available = new List<AuthProviderVM>();

            foreach (var prov in SupportedProviders)
                if(prov.TryActivate(config, authBuilder))
                    available.Add(prov);

            AvailableProviders = available;
        }
    }
}
