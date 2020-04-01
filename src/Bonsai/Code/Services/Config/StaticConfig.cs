namespace Bonsai.Code.Services.Config
{
    /// <summary>
    /// Global configuration properties defined in appsettings.json.
    /// </summary>
    public class StaticConfig
    {
        public ConnectionStringsConfig ConnectionStrings { get; set; }
        public DebugConfig Debug { get; set; }
        public WebServerConfig WebServer { get; set; }
        public DemoModeConfig DemoMode { get; set; }
        public AuthConfig Auth { get; set; }
    }

    /// <summary>
    /// Connection string properties.
    /// </summary>
    public class ConnectionStringsConfig
    {
        public string Database { get; set; }
    }

    /// <summary>
    /// Properties related to debugging.
    /// </summary>
    public class DebugConfig
    {
        public bool DetailedExceptions { get; set; }
    }

    /// <summary>
    /// Webserver properties.
    /// </summary>
    public class WebServerConfig
    {
        public bool RequireHttps { get; set; }
        public long? MaxUploadSize { get; set; }
    }

    /// <summary>
    /// Demo mode configuration options.
    /// </summary>
    public class DemoModeConfig
    {
        public bool Enabled { get; set; }
        public bool CreateDefaultPages { get; set; }
        public bool CreateDefaultAdmin { get; set; }
        public bool ClearOnStartup { get; set; }
        public string YandexMetrikaId { get; set; }
    }

    /// <summary>
    /// Authorization options properties.
    /// </summary>
    public class AuthConfig
    {
        public bool AllowPasswordAuth { get; set; }
        public FacebookAuthConfig Facebook { get; set; }
        public GenericAuthConfig Google { get; set; }
        public GenericAuthConfig Vkontakte { get; set; }
        public GenericAuthConfig Yandex { get; set; }
    }

    /// <summary>
    /// Facebook-related authorization properties.
    /// </summary>
    public class FacebookAuthConfig
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }

    /// <summary>
    /// Authorization properties for all other providers.
    /// </summary>
    public class GenericAuthConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
