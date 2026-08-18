namespace Bonsai.Data.Models;

/// <summary>
/// Purpose of a cryptographic key used by the OAuth server.
/// </summary>
public enum OAuthKeyPurpose
{
    /// <summary>
    /// Key for signing MCP tokens.
    /// </summary>
    Signing,

    /// <summary>
    /// Key for encrypting MCP tokens.
    /// </summary>
    Encryption
}
