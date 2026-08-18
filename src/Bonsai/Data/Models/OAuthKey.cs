using System;
using System.ComponentModel.DataAnnotations;

namespace Bonsai.Data.Models;

/// <summary>
/// A cryptographic key used by the OAuth server (OpenIddict) to sign and encrypt MCP access tokens.
/// Persisted in the database so that tokens issued to authorized agents remain valid across application
/// restarts (otherwise a new key would be generated on every startup and force agents to re-authorize).
/// </summary>
public class OAuthKey
{
    /// <summary>
    /// Purpose of the key: signing or encryption.
    /// </summary>
    [Key]
    public OAuthKeyPurpose Purpose { get; set; }

    /// <summary>
    /// Base64-encoded PKCS#8 RSA private key.
    /// </summary>
    [Required]
    public string PrivateKey { get; set; }

    /// <summary>
    /// Timestamp when the key was generated.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
