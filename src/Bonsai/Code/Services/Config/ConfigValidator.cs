using Bonsai.Code.Utils.Validation;

namespace Bonsai.Code.Services.Config
{
    /// <summary>
    /// Helper class for ensuring the configuration is valid.
    /// </summary>
    public static class ConfigValidator
    {
        /// <summary>
        /// Checks that the config has everything set correctly and throws an exception otherwise.
        /// </summary>
        public static void EnsureValid(StaticConfig config)
        {
            var validator = new Validator();

            if (config.Auth is { } auth)
            {
                if (auth.AllowPasswordAuth == false && auth.Facebook == null && auth.Google == null && auth.Vkontakte == null && auth.Yandex == null)
                    validator.Add(nameof(StaticConfig.Auth), "All authorization options are disabled. Please allow at lease one (e.g. AllowPasswordAuth = true)");

                if (auth.Facebook is { } fb)
                {
                    if(string.IsNullOrWhiteSpace(fb.AppId))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(fb.AppId), "Application ID is required for Facebook auth.");

                    if (string.IsNullOrWhiteSpace(fb.AppSecret))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(fb.AppSecret), "Application secret is required for Facebook auth.");
                }

                if (auth.Google is { } google)
                {
                    if (string.IsNullOrWhiteSpace(google.ClientId))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(google.ClientId), "Client ID is required for Google auth.");

                    if (string.IsNullOrWhiteSpace(google.ClientSecret))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(google.ClientSecret), "Client secret is required for Google auth.");
                }

                if (auth.Vkontakte is { } vk)
                {
                    if (string.IsNullOrWhiteSpace(vk.ClientId))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(vk.ClientId), "Client ID is required for Vkontakte auth.");

                    if (string.IsNullOrWhiteSpace(vk.ClientSecret))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(vk.ClientSecret), "Client secret is required for Vkontakte auth.");
                }

                if (auth.Yandex is { } ya)
                {
                    if (string.IsNullOrWhiteSpace(ya.ClientId))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(ya.ClientId), "Client ID is required for Yandex auth.");

                    if (string.IsNullOrWhiteSpace(ya.ClientSecret))
                        validator.Add(nameof(StaticConfig.Auth) + "__" + nameof(ya.ClientSecret), "Client secret is required for Yandex auth.");
                }
            }
            else
            {
                validator.Add(nameof(StaticConfig.Auth), "All authorization options are disabled. Please allow at lease one (e.g. AllowPasswordAuth = true)");
            }

            if (config.ConnectionStrings is { } conns)
            {
                if (conns.UseEmbeddedDatabase)
                {
                    if(string.IsNullOrWhiteSpace(conns.EmbeddedDatabase))
                        validator.Add(nameof(StaticConfig.ConnectionStrings) + "__" + nameof(conns.EmbeddedDatabase), "Embedded database connection string is missing.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(conns.Database))
                        validator.Add(nameof(StaticConfig.ConnectionStrings) + "__" + nameof(conns.Database), "Database connection string is missing.");
                }
            }
            else
            {
                validator.Add(nameof(StaticConfig.ConnectionStrings), "Database connection strings configuration is missing. The 'ConnectionStrings__UseEmbeddedDatabase' flag and either 'ConnectionStrings__EmbeddedDatabase' or 'ConnectionStrings__Database' are required.");
            }

            validator.ThrowIfInvalid("Bonsai configuration is invalid!");
        }
    }
}
