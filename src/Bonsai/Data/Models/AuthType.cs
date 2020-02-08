namespace Bonsai.Data.Models
{
    /// <summary>
    /// Supported authorization types.
    /// </summary>
    public enum AuthType
    {
        /// <summary>
        /// Authorization via a social network.
        /// </summary>
        ExternalProvider = 0,

        /// <summary>
        /// Authorization via login/password.
        /// </summary>
        Password = 1
    }
}
