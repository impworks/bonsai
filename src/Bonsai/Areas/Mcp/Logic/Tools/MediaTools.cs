using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.ViewModels.Media;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Data;
using Bonsai.Data.Models;
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
    AppDbContext db)
{
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

        await mediaManagerService.UpdateAsync(current, userContext.Principal);
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
        await mediaManagerService.RemoveAsync(id, userContext.Principal);
        await db.SaveChangesAsync();

        return new DeleteMediaResult
        {
            Id = id,
            Success = true
        };
    }
}

#region Input/Result Types

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
