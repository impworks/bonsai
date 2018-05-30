namespace Bonsai.Areas.Front.Logic.Auth
{
    /// <summary>
    /// Details of an external provider's authentication.
    /// </summary>
    public class ExternalLoginData
    {
        public ExternalLoginData(string loginProvider, string providerKey)
        {
            LoginProvider = loginProvider;
            ProviderKey = providerKey;
        }

        /// <summary>
        /// External provider: Facebook, Google, etc.
        /// </summary>
        public string LoginProvider { get; }

        /// <summary>
        /// User's personal key returned by the provider.
        /// </summary>
        public string ProviderKey { get; }
    }
}
