using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bonsai.Code.Services.Config;
using Bonsai.Data.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace Bonsai.Areas.Mcp.Controllers;

/// <summary>
/// OAuth 2.0/2.1 authorization and token endpoints for MCP.
/// </summary>
[Area("Mcp")]
public class OAuthController(
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictAuthorizationManager authorizationManager,
    SignInManager<AppUser> signInManager,
    UserManager<AppUser> userManager,
    BonsaiConfigService configService)
    : Controller
{

    /// <summary>
    /// Authorization endpoint - handles OAuth authorization requests.
    /// </summary>
    [HttpGet("~/oauth/authorize")]
    [HttpPost("~/oauth/authorize")]
    public async Task<IActionResult> Authorize()
    {
        if (!configService.GetDynamicConfig().McpEnabled)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "MCP server is disabled. Enable it in the admin configuration."
                }));
        }

        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties
                {
                    RedirectUri = Request.PathBase + Request.Path + QueryString.Create(
                        Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList())
                });
        }

        var user = await userManager.GetUserAsync(result.Principal) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        var application = await applicationManager.FindByClientIdAsync(request.ClientId!) ??
            throw new InvalidOperationException("The application details cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application
        var authorizations = await authorizationManager.FindAsync(
            subject: await userManager.GetUserIdAsync(user),
            client: await applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException(),
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        var principal = await CreateUserPrincipalAsync(user, request);

        // Automatically grant consent for MCP clients (since they're registered dynamically)
        // In a more restrictive scenario, you might want to show a consent screen
        var authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            principal: principal,
            subject: await userManager.GetUserIdAsync(user),
            client: await applicationManager.GetIdAsync(application) ?? throw new InvalidOperationException(),
            type: AuthorizationTypes.Permanent,
            scopes: principal.GetScopes());

        principal.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Token endpoint - exchanges authorization codes for access tokens.
    /// </summary>
    [HttpPost("~/oauth/token")]
    public async Task<IActionResult> Exchange()
    {
        if (!configService.GetDynamicConfig().McpEnabled)
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ServerError,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "MCP server is disabled. Enable it in the admin configuration."
                }));
        }

        var request = HttpContext.GetOpenIddictServerRequest()
                      ?? throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var user = await userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? "");
            if (user is null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }));
            }

            // Ensure the user is still allowed to sign in
            if (!await signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            var principal = await CreateUserPrincipalAsync(user, request);

            // Set the authorization id from the original token
            var authorizationId = result.Principal?.GetAuthorizationId();
            if (!string.IsNullOrEmpty(authorizationId))
                principal.SetAuthorizationId(authorizationId);

            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsClientCredentialsGrantType())
        {
            // For client credentials, we create a minimal principal
            var application = await applicationManager.FindByClientIdAsync(request.ClientId!) ??
                throw new InvalidOperationException("The application details cannot be found.");

            var identity = new ClaimsIdentity(
                authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetClaim(Claims.Subject, await applicationManager.GetClientIdAsync(application));
            identity.SetClaim(Claims.Name, await applicationManager.GetDisplayNameAsync(application));

            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    private async Task<ClaimsPrincipal> CreateUserPrincipalAsync(AppUser user, OpenIddictRequest request)
    {
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;

        identity.SetClaim(Claims.Subject, user.Id);

        if (!string.IsNullOrEmpty(user.UserName))
            identity.SetClaim(Claims.Name, user.UserName);
        if (!string.IsNullOrEmpty(user.Email))
            identity.SetClaim(Claims.Email, user.Email);

        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
            identity.AddClaim(new Claim(Claims.Role, role));

        var userRole = UserRole.User; // Default
        if (roles.Contains(nameof(UserRole.Admin)))
            userRole = UserRole.Admin;
        else if (roles.Contains(nameof(UserRole.Editor)))
            userRole = UserRole.Editor;
        else if (roles.Contains(nameof(UserRole.User)))
            userRole = UserRole.User;
        else
            userRole = UserRole.Unvalidated;

        identity.AddClaim(new Claim("bonsai_role", userRole.ToString()));

        principal.SetScopes(request.GetScopes());
        principal.SetResources("bonsai-mcp");
        principal.SetDestinations(GetDestinations);

        return principal;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, identity tokens or both.

        switch (claim.Type)
        {
            case Claims.Name:
            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject?.HasScope(Scopes.Profile) == true || claim.Subject?.HasScope(Scopes.Email) == true)
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
            case "bonsai_role":
            case Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}
