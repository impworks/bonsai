using Bonsai.Data.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;

namespace Bonsai.Areas.Mcp.Logic.Auth;

/// <summary>
/// Registers the persistent signing and encryption credentials on the OpenIddict server options.
/// This replaces the ephemeral keys that were previously regenerated on every startup, which invalidated
/// the tokens of already-authorized MCP agents.
/// </summary>
public class ConfigureOpenIddictServerKeys(OAuthKeyManager keyManager) : IConfigureOptions<OpenIddictServerOptions>
{
    public void Configure(OpenIddictServerOptions options)
    {
        options.SigningCredentials.Add(new SigningCredentials(
            keyManager.GetKey(OAuthKeyPurpose.Signing),
            SecurityAlgorithms.RsaSha256));

        options.EncryptionCredentials.Add(new EncryptingCredentials(
            keyManager.GetKey(OAuthKeyPurpose.Encryption),
            SecurityAlgorithms.RsaOAEP,
            SecurityAlgorithms.Aes256CbcHmacSha512));
    }
}
