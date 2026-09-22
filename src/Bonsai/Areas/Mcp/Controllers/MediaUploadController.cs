using System;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.MediaHandlers;
using Bonsai.Areas.Admin.Utils;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Code.Infrastructure.Attributes;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Services.Jobs;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Bonsai.Areas.Mcp.Controllers;

/// <summary>
/// Accepts media uploads authorized by one-time tokens issued via the "create_media_upload" MCP tool.
/// The request body is the raw file content (e.g. "curl -T file.jpg URL").
/// </summary>
[ApiController]
public class MediaUploadController(
    MediaUploadTokenService tokenService,
    MediaManagerService mediaSvc,
    BonsaiConfigService configService,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IBackgroundJobService jobs,
    AppDbContext db,
    ILogger logger
) : ControllerBase
{
    /// <summary>
    /// Saves the request body as a new media file.
    /// </summary>
    [HttpPut("/mcp/upload/{token}")]
    [HttpPost("/mcp/upload/{token}")]
    [ConfigurableRequestSizeLimit]
    public async Task<IActionResult> Upload()
    {
        // the token is not bound as an action parameter on purpose:
        // parameter binding makes MVC read the whole body into memory if the client sends form content
        var token = RouteData.Values["token"] as string;

        if (!configService.GetDynamicConfig().McpEnabled)
            return Error(StatusCodes.Status403Forbidden, "MCP server is disabled. Enable it in the admin configuration.");

        if (Request.HasFormContentType)
            return Error(StatusCodes.Status415UnsupportedMediaType, "Send the raw file content as the request body (e.g. 'curl -T <file> <url>'), not a form.");

        if (Request.ContentLength == 0)
            return Error(StatusCodes.Status400BadRequest, "The request body is empty.");

        var ticket = tokenService.TryRedeem(token);
        if (ticket == null)
            return Error(StatusCodes.Status404NotFound, "The upload token is invalid, expired, or has already been used. Request a new one via the 'create_media_upload' tool.");

        var user = await userManager.FindByIdAsync(ticket.UserId);
        if (user == null || !(await userManager.IsInRoleAsync(user, nameof(UserRole.Editor)) || await userManager.IsInRoleAsync(user, nameof(UserRole.Admin))))
            return Error(StatusCodes.Status403Forbidden, "The user who requested the upload no longer has the Editor role.");

        try
        {
            var principal = await signInManager.CreateUserPrincipalAsync(user);
            var result = await mediaSvc.UploadAsync(ticket.Request, Request.Body, ticket.FileName, ticket.MimeType, null, principal, isAIGenerated: true);
            await db.SaveChangesAsync();

            if (!result.IsProcessed)
                await jobs.RunAsync(JobBuilder.For<MediaEncoderJob>().WithArgs(result.Id));

            return Ok(new
            {
                result.Id,
                result.Key,
                result.IsProcessed,
                ThumbnailPath = $"{Request.Scheme}://{Request.Host}{Url.Content(result.ThumbnailPath)}"
            });
        }
        catch (BadHttpRequestException ex) when (ex.StatusCode == StatusCodes.Status413PayloadTooLarge)
        {
            return Error(StatusCodes.Status413PayloadTooLarge, "The file exceeds the maximum upload size.");
        }
        catch (UploadException ex)
        {
            return Error(StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to process an MCP media upload.");
            return Error(StatusCodes.Status500InternalServerError, "Failed to process the upload: " + ex.Message);
        }
    }

    /// <summary>
    /// Returns a JSON error response.
    /// </summary>
    private IActionResult Error(int statusCode, string message)
    {
        return StatusCode(statusCode, new { Error = true, Description = message });
    }
}
