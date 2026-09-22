using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Code.Services.Config;
using Bonsai.Code.Utils.Date;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using ModelContextProtocol.Server;

namespace Bonsai.Areas.Mcp.Logic.Tools;

/// <summary>
/// MCP tools for managing media files.
/// </summary>
[McpServerToolType]
public class MediaTools(
    MediaPresenterService mediaPresenterService,
    MediaManagerService mediaManagerService,
    McpToolAuthorizationService authService,
    McpUserContext userContext,
    MediaUploadTokenService uploadTokenService,
    BonsaiConfigService configService,
    IHttpContextAccessor httpContextAccessor,
    AppDbContext db)
{
    private static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();

    /// <summary>
    /// Creates a one-time upload URL for a new media file.
    /// </summary>
    [McpServerTool(Name = "create_media_upload")]
    [Description("""
        Create a one-time URL for uploading a new media file (photo, video, or PDF document). Requires Editor role.
        File contents cannot be passed through MCP: send the raw file as the body of an HTTP PUT request to the returned URL,
        e.g. `curl --fail-with-body -T "<local file path>" "<uploadUrl>"` (in PowerShell, call `curl.exe`, not the `curl` alias).
        The URL expires after a short time and can only be used once. The response of the PUT request is JSON with the new media's id and key,
        which can then be used with update_media (description, tags) or as mainPhotoKey in create_page/update_page.
        Videos are processed in the background: check isProcessed via read_media.
        """)]
    public async Task<CreateMediaUploadResult> CreateMediaUpload(
        [Description("Name of the file including extension (e.g. 'IMG_1234.jpg'). The extension is kept for the stored file.")] string fileName,
        [Description("MIME type of the file (optional, detected from the extension if omitted). Supported: image/jpeg, image/png, common video types, application/pdf")] string mimeType = null,
        [Description("File size in bytes (optional, used to reject files that exceed the server's upload limit before sending them)")] long? fileSize = null,
        [Description("Media title (optional)")] string title = null,
        [Description("Date in any of the formats: YYYY.MM.DD = precise date, YYYY.MM.?? = year and month, YYYY.??.?? = year, YYY?.YY.YY = decade, ????.MM.DD = date without year (optional, extracted from photo metadata if omitted)")] string date = null,
        [Description("Location page ID (optional)")] string locationId = null,
        [Description("Event page ID (optional)")] string eventId = null)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        fileName = Path.GetFileName(fileName ?? "");
        var ext = Path.GetExtension(fileName);
        if (!Regex.IsMatch(ext, @"^\.[A-Za-z0-9]{1,10}$"))
            throw new ArgumentException($"File name '{fileName}' must have a valid extension (e.g. '.jpg').");

        if (string.IsNullOrEmpty(mimeType) && !ContentTypeProvider.TryGetContentType(fileName, out mimeType))
            throw new ArgumentException($"Cannot detect the MIME type of '{fileName}'. Specify mimeType explicitly.");

        if (mediaManagerService.GetMediaType(mimeType) == null)
            throw new ArgumentException($"Unsupported file type: {mimeType}.");

        var maxSize = configService.GetStaticConfig().WebServer?.MaxUploadSize;
        if (fileSize > maxSize)
            throw new ArgumentException($"The file is too large ({fileSize} bytes). Maximum upload size is {maxSize} bytes.");

        if (!string.IsNullOrEmpty(date) && FuzzyDate.TryParse(date) == null)
            throw new ArgumentException($"Invalid date: '{date}'.");

        var request = new MediaUploadRequestVM
        {
            Title = title,
            Date = date,
            Location = locationId,
            Event = eventId
        };
        var ticket = uploadTokenService.Create(userContext.UserId, fileName, mimeType, request);

        var http = httpContextAccessor.HttpContext.Request;
        var url = $"{http.Scheme}://{http.Host}{http.PathBase}/mcp/upload/{ticket.Token}";

        return new CreateMediaUploadResult
        {
            UploadUrl = url,
            Method = "PUT",
            ExpiresAt = ticket.ExpiresAt,
            CurlCommand = $"curl --fail-with-body -T \"<local file path>\" \"{url}\""
        };
    }

    /// <summary>
    /// Lists media files with optional filters.
    /// </summary>
    [McpServerTool(Name = "list_media")]
    [Description("List media files (photos, videos, documents) with optional filters.")]
    public async Task<ListMediaResult> ListMedia(
        [Description("Filter by media types (comma-separated: Photo, Video, Document)")] string types = null,
        [Description("Filter by entity ID - show only media tagged with this page")] string entityId = null,
        [Description("Search query to filter by title")] string searchQuery = null,
        [Description("Field to order by (UploadDate, Date, Title, Tags)")] string orderBy = "UploadDate",
        [Description("Order descending (default: true)")] bool orderDescending = true,
        [Description("Page number for pagination (0-based, default: 0)")] int page = 0)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var typesList = string.IsNullOrEmpty(types)
            ? null
            : types.Split(',')
                   .Select(t => Enum.TryParse<MediaType>(t.Trim(), true, out var mt) ? mt : (MediaType?)null)
                   .Where(t => t.HasValue)
                   .Select(t => t.Value)
                   .ToArray();

        var request = new MediaListRequestVM
        {
            Types = typesList,
            EntityId = string.IsNullOrEmpty(entityId) ? null : Guid.Parse(entityId),
            SearchQuery = searchQuery,
            OrderBy = orderBy,
            OrderDescending = orderDescending,
            Page = page
        };

        var result = await mediaManagerService.GetMediaAsync(request);

        return new ListMediaResult
        {
            Media = result.Items.Select(m => new MediaListItem
            {
                Id = m.Id,
                Key = m.Key,
                Title = m.Title,
                Type = m.Type.ToString(),
                ThumbnailPath = m.ThumbnailUrl,
                UploadDate = m.UploadDate,
                Date = m.Date?.ToString(),
                IsProcessed = m.IsProcessed
            }).ToList(),
            TotalPages = result.PageCount,
            CurrentPage = request.Page,
            EntityTitle = result.EntityTitle
        };
    }

    /// <summary>
    /// Reads media file details.
    /// </summary>
    [McpServerTool(Name = "read_media")]
    [Description("Read details of a media file by its key.")]
    public async Task<ReadMediaResult> ReadMedia(
        [Description("The media key (e.g., 'media-12345678')")] string key)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var media = await mediaPresenterService.GetMediaAsync(key);

        return new ReadMediaResult
        {
            Id = media.Id,
            Type = media.Type.ToString(),
            Title = media.Title,
            Description = media.Description,
            Date = media.Date?.ToString(),
            OriginalPath = media.OriginalPath,
            PreviewPath = media.PreviewPath,
            IsProcessed = media.IsProcessed,
            Tags = media.Tags?.Select(t => new MediaTagInfo
            {
                TagId = t.TagId,
                PageId = t.Page?.Id,
                PageKey = t.Page?.Key,
                PageTitle = t.Page?.Title,
                Coordinates = t.Rect.HasValue
                    ? $"{t.Rect.Value.X};{t.Rect.Value.Y};{t.Rect.Value.Width};{t.Rect.Value.Height}"
                    : null
            }).ToList() ?? [],
            Location = media.Location != null
                ? new PageReference { Id = media.Location.Id ?? Guid.Empty, Key = media.Location.Key, Title = media.Location.Title }
                : null,
            Event = media.Event != null
                ? new PageReference { Id = media.Event.Id ?? Guid.Empty, Key = media.Event.Key, Title = media.Event.Title }
                : null
        };
    }

    /// <summary>
    /// Updates media metadata.
    /// </summary>
    [McpServerTool(Name = "update_media")]
    [Description("Update media metadata (title, description, date, tags). Requires Editor role.")]
    public async Task<UpdateMediaResult> UpdateMedia(
        [Description("Media ID (GUID)")] string mediaId,
        [Description("New title (optional)")] string title = null,
        [Description("New description in Markdown (optional)")] string description = null,
        [Description("New date in any of the formats: YYYY.MM.DD = precise date, YYYY.MM.?? = year and month, YYYY.??.?? = year, YYY?.YY.YY = decade, ????.MM.DD = date without year (optional)")] string date = null,
        [Description("Location page ID (optional)")] string locationId = null,
        [Description("Event page ID (optional)")] string eventId = null)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(mediaId);
        var current = await mediaManagerService.RequestUpdateAsync(id);

        if (title != null)
            current.Title = title;

        if (description != null)
            current.Description = description;

        if (date != null)
            current.Date = date;

        if (locationId != null)
            current.Location = locationId;

        if (eventId != null)
            current.Event = eventId;

        await mediaManagerService.UpdateAsync(current, userContext.Principal, isAIGenerated: true);
        await db.SaveChangesAsync();

        return new UpdateMediaResult
        {
            Id = id,
            Success = true
        };
    }

    /// <summary>
    /// Deletes a media file (soft delete).
    /// </summary>
    [McpServerTool(Name = "delete_media")]
    [Description("Delete a media file (soft delete, can be restored). Requires Editor role.")]
    public async Task<DeleteMediaResult> DeleteMedia(
        [Description("Media ID (GUID)")] string mediaId)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(mediaId);
        await mediaManagerService.RemoveAsync(id, userContext.Principal, isAIGenerated: true);
        await db.SaveChangesAsync();

        return new DeleteMediaResult
        {
            Id = id,
            Success = true
        };
    }
}

#region Input/Result Types

public class CreateMediaUploadResult
{
    public string UploadUrl { get; set; }
    public string Method { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public string CurlCommand { get; set; }
}

public class ListMediaResult
{
    public List<MediaListItem> Media { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public string EntityTitle { get; set; }
}

public class MediaListItem
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string ThumbnailPath { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public string Date { get; set; }
    public bool IsProcessed { get; set; }
}

public class ReadMediaResult
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Date { get; set; }
    public string OriginalPath { get; set; }
    public string PreviewPath { get; set; }
    public bool IsProcessed { get; set; }
    public List<MediaTagInfo> Tags { get; set; }
    public PageReference Location { get; set; }
    public PageReference Event { get; set; }
}

public class MediaTagInfo
{
    public Guid TagId { get; set; }
    public Guid? PageId { get; set; }
    public string PageKey { get; set; }
    public string PageTitle { get; set; }
    public string Coordinates { get; set; }
}

public class PageReference
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
}

public class UpdateMediaResult
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
}

public class DeleteMediaResult
{
    public Guid Id { get; set; }
    public bool Success { get; set; }
}

#endregion
