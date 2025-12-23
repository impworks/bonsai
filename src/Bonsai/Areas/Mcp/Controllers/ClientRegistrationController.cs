using System;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenIddict.Abstractions;

namespace Bonsai.Areas.Mcp.Controllers;

/// <summary>
/// Dynamic Client Registration endpoint (RFC 7591) for MCP OAuth clients.
/// This allows MCP clients like Claude to register themselves dynamically.
/// </summary>
[Area("Mcp")]
[Route("oauth/register")]
[ApiController]
public class ClientRegistrationController(
    IOpenIddictApplicationManager applicationManager,
    BonsaiConfigService configService) : ControllerBase
{
    /// <summary>
    /// Register a new OAuth client dynamically (RFC 7591).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Register([FromBody] ClientRegistrationRequest request)
    {
        if (!configService.GetDynamicConfig().McpEnabled)
        {
            return BadRequest(new ClientRegistrationError
            {
                Error = "server_error",
                ErrorDescription = "MCP server is disabled. Enable it in the admin configuration."
            });
        }

        if (request == null)
        {
            return BadRequest(new ClientRegistrationError
            {
                Error = "invalid_client_metadata",
                ErrorDescription = "The client registration request is invalid."
            });
        }

        if (request.RedirectUris == null || request.RedirectUris.Length == 0)
        {
            return BadRequest(new ClientRegistrationError
            {
                Error = "invalid_redirect_uri",
                ErrorDescription = "At least one redirect_uri is required."
            });
        }

        foreach (var uri in request.RedirectUris)
        {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri))
            {
                return BadRequest(new ClientRegistrationError
                {
                    Error = "invalid_redirect_uri",
                    ErrorDescription = $"Invalid redirect_uri: {uri}"
                });
            }

            var isLocalhost = parsedUri.Host == "localhost" || parsedUri.Host == "127.0.0.1";
            var isHttps = parsedUri.Scheme == "https";

            if (!isLocalhost && !isHttps)
            {
                return BadRequest(new ClientRegistrationError
                {
                    Error = "invalid_redirect_uri",
                    ErrorDescription = "Redirect URIs must use HTTPS or be localhost."
                });
            }
        }

        var clientId = $"mcp_{Guid.NewGuid():N}";
        var clientSecret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var clientType = OpenIddictConstants.ClientTypes.Confidential;

        
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientSecret = clientType == OpenIddictConstants.ClientTypes.Confidential ? clientSecret : null,
            ClientType = clientType,
            DisplayName = request.ClientName ?? $"MCP Client {clientId}",
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit, // Auto-approve for MCP clients
        };

        foreach (var uri in request.RedirectUris)
            descriptor.RedirectUris.Add(new Uri(uri));

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);

        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Email);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Scopes.Profile);
        descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + "mcp");

        descriptor.Requirements.Add(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange);

        try
        {
            await applicationManager.CreateAsync(descriptor);
        }
        catch (Exception ex)
        {
            return BadRequest(new ClientRegistrationError
            {
                Error = "server_error",
                ErrorDescription = $"Failed to register client: {ex.Message}"
            });
        }

        var response = new ClientRegistrationResponse
        {
            ClientId = clientId,
            ClientSecret = clientType == OpenIddictConstants.ClientTypes.Confidential ? clientSecret : null,
            ClientSecretExpiresAt = 0, // sic! Never expires
            ClientIdIssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            RedirectUris = request.RedirectUris,
            TokenEndpointAuthMethod = request.TokenEndpointAuthMethod ?? "client_secret_post",
            GrantTypes = ["authorization_code", "refresh_token"],
            ResponseTypes = ["code"],
            ClientName = descriptor.DisplayName,
            Scope = "openid profile email mcp"
        };

        return Created($"/oauth/register/{clientId}", response);
    }

    /// <summary>
    /// Get client registration information.
    /// </summary>
    [HttpGet("{clientId}")]
    public async Task<IActionResult> GetClient(string clientId)
    {
        if (!configService.GetDynamicConfig().McpEnabled)
        {
            return BadRequest(new ClientRegistrationError
            {
                Error = "server_error",
                ErrorDescription = "MCP server is disabled. Enable it in the admin configuration."
            });
        }

        var application = await applicationManager.FindByClientIdAsync(clientId);
        if (application == null)
        {
            return NotFound(new ClientRegistrationError
            {
                Error = "invalid_client",
                ErrorDescription = "Client not found."
            });
        }

        var redirectUris = await applicationManager.GetRedirectUrisAsync(application);
        var displayName = await applicationManager.GetDisplayNameAsync(application);
        var clientType = await applicationManager.GetClientTypeAsync(application);

        return Ok(new ClientRegistrationResponse
        {
            ClientId = clientId,
            ClientSecret = null, // Never return the secret
            RedirectUris = [.. redirectUris],
            ClientName = displayName,
            TokenEndpointAuthMethod = clientType == OpenIddictConstants.ClientTypes.Public ? "none" : "client_secret_post",
            GrantTypes = ["authorization_code", "refresh_token"],
            ResponseTypes = ["code"],
            Scope = "openid profile email mcp"
        });
    }
}

#region Request/Response Models

public class ClientRegistrationRequest
{
    [JsonProperty("redirect_uris")]
    public string[] RedirectUris { get; set; }

    [JsonProperty("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; }

    [JsonProperty("grant_types")]
    public string[] GrantTypes { get; set; }

    [JsonProperty("response_types")]
    public string[] ResponseTypes { get; set; }

    [JsonProperty("client_name")]
    public string ClientName { get; set; }
}

public class ClientRegistrationResponse
{
    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; }

    [JsonProperty("client_secret_expires_at")]
    public long ClientSecretExpiresAt { get; set; }

    [JsonProperty("client_id_issued_at")]
    public long ClientIdIssuedAt { get; set; }

    [JsonProperty("redirect_uris")]
    public string[] RedirectUris { get; set; }

    [JsonProperty("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; }

    [JsonProperty("grant_types")]
    public string[] GrantTypes { get; set; }

    [JsonProperty("response_types")]
    public string[] ResponseTypes { get; set; }

    [JsonProperty("client_name")]
    public string ClientName { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }
}

public class ClientRegistrationError
{
    [JsonProperty("error")]
    public string Error { get; set; }

    [JsonProperty("error_description")]
    public string ErrorDescription { get; set; }
}

#endregion
