using System.Text.Json.Serialization;
using Bonsai.Code.Services.Config;
using Microsoft.AspNetCore.Mvc;

namespace Bonsai.Areas.Mcp.Controllers;

/// <summary>
/// OAuth 2.0 Authorization Server Metadata endpoint (RFC 8414).
/// This is required by MCP spec for clients to discover OAuth endpoints.
/// </summary>
[ApiController]
public class OAuthMetadataController(BonsaiConfigService configService) : ControllerBase
{
    /// <summary>
    /// Returns OAuth 2.0 Authorization Server Metadata (RFC 8414).
    /// </summary>
    [HttpGet("/.well-known/oauth-authorization-server")]
    [HttpGet("/.well-known/oauth-authorization-server/{path}")]
    public IActionResult GetMetadata(string path = null)
    {
        if (!configService.GetDynamicConfig().McpEnabled)
        {
            return BadRequest(new { error = "server_error", error_description = "MCP server is disabled. Enable it in the admin configuration." });
        }

        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var metadata = new OAuthServerMetadata
        {
            Issuer = baseUrl,
            AuthorizationEndpoint = $"{baseUrl}/oauth/authorize",
            TokenEndpoint = $"{baseUrl}/oauth/token",
            UserinfoEndpoint = $"{baseUrl}/oauth/userinfo",
            RegistrationEndpoint = $"{baseUrl}/oauth/register",
            JwksUri = $"{baseUrl}/.well-known/jwks",
            ScopesSupported = ["openid", "profile", "email", "mcp"],
            ResponseTypesSupported = ["code"],
            ResponseModesSupported = ["query", "fragment"],
            GrantTypesSupported = ["authorization_code", "refresh_token", "client_credentials"],
            TokenEndpointAuthMethodsSupported = ["client_secret_post", "client_secret_basic", "none"],
            CodeChallengeMethodsSupported = ["S256"],
            SubjectTypesSupported = ["public"],
            IdTokenSigningAlgValuesSupported = ["RS256"],
            ClaimsSupported = ["sub", "name", "email", "email_verified", "bonsai_role"],
            ServiceDocumentation = $"{baseUrl}/mcp/api-keys"
        };

        return Ok(metadata);
    }

    /// <summary>
    /// Redirect from OpenID Connect discovery to OAuth metadata.
    /// OpenIddict automatically handles /.well-known/openid-configuration,
    /// but MCP clients may look at either endpoint.
    /// </summary>
    [HttpGet("/.well-known/openid-configuration")]
    public IActionResult GetOpenIdConfiguration()
    {
        // Let OpenIddict handle this - this method is here just for documentation
        // OpenIddict will intercept this request automatically
        return NotFound();
    }
}

#region Metadata Models

public class OAuthServerMetadata
{
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; }

    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; }

    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; }

    [JsonPropertyName("userinfo_endpoint")]
    public string UserinfoEndpoint { get; set; }

    [JsonPropertyName("registration_endpoint")]
    public string RegistrationEndpoint { get; set; }

    [JsonPropertyName("jwks_uri")]
    public string JwksUri { get; set; }

    [JsonPropertyName("scopes_supported")]
    public string[] ScopesSupported { get; set; }

    [JsonPropertyName("response_types_supported")]
    public string[] ResponseTypesSupported { get; set; }

    [JsonPropertyName("response_modes_supported")]
    public string[] ResponseModesSupported { get; set; }

    [JsonPropertyName("grant_types_supported")]
    public string[] GrantTypesSupported { get; set; }

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public string[] TokenEndpointAuthMethodsSupported { get; set; }

    [JsonPropertyName("code_challenge_methods_supported")]
    public string[] CodeChallengeMethodsSupported { get; set; }

    [JsonPropertyName("subject_types_supported")]
    public string[] SubjectTypesSupported { get; set; }

    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgValuesSupported { get; set; }

    [JsonPropertyName("claims_supported")]
    public string[] ClaimsSupported { get; set; }

    [JsonPropertyName("service_documentation")]
    public string ServiceDocumentation { get; set; }
}

#endregion
