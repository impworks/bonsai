using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic.Changesets;
using Bonsai.Areas.Admin.ViewModels.Changesets;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Data;
using Bonsai.Data.Models;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace Bonsai.Areas.Mcp.Logic.Tools;

/// <summary>
/// MCP tools for accessing activity and changesets.
/// </summary>
[McpServerToolType]
public class ActivityTools(
    ChangesetsManagerService changesetsService,
    AppDbContext db,
    McpToolAuthorizationService authService)
{
    /// <summary>
    /// Lists changesets (history of changes).
    /// </summary>
    [McpServerTool(Name = "list_changesets")]
    [Description("List changesets showing the history of changes to pages, media, and relations.")]
    public async Task<ListChangesetsResult> ListChangesets(
        [Description("Filter by entity types (comma-separated: Page, Media, Relation)")] string entityTypes = null,
        [Description("Filter by change types (comma-separated: Created, Updated, Removed, Restored)")] string changesetTypes = null,
        [Description("Filter by entity ID (show changes for specific page/media/relation)")] string entityId = null,
        [Description("Filter by user ID (show changes by specific user)")] string userId = null,
        [Description("Search query to filter by entity title")] string searchQuery = null,
        [Description("Order by field (Date, Author)")] string orderBy = "Date",
        [Description("Order descending (default: true)")] bool orderDescending = true,
        [Description("Page number for pagination (0-based, default: 0)")] int page = 0)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var entityTypesList = string.IsNullOrEmpty(entityTypes)
            ? null
            : entityTypes.Split(',')
                         .Select(t => Enum.TryParse<ChangesetEntityType>(t.Trim(), true, out var et) ? et : (ChangesetEntityType?)null)
                         .Where(t => t.HasValue)
                         .Select(t => t.Value)
                         .ToArray();

        var changeTypesList = string.IsNullOrEmpty(changesetTypes)
            ? null
            : changesetTypes.Split(',')
                            .Select(t => Enum.TryParse<ChangesetType>(t.Trim(), true, out var ct) ? ct : (ChangesetType?)null)
                            .Where(t => t.HasValue)
                            .Select(t => t.Value)
                            .ToArray();

        var request = new ChangesetsListRequestVM
        {
            EntityTypes = entityTypesList,
            ChangesetTypes = changeTypesList,
            EntityId = string.IsNullOrEmpty(entityId) ? null : Guid.Parse(entityId),
            UserId = userId,
            SearchQuery = searchQuery,
            OrderBy = orderBy,
            OrderDescending = orderDescending,
            Page = page
        };

        var result = await changesetsService.GetChangesetsAsync(request);

        return new ListChangesetsResult
        {
            Changesets = result.Items.Select(c => new ChangesetListItem
            {
                Id = c.Id,
                Date = c.Date.DateTime,
                ChangeType = c.ChangeType.ToString(),
                EntityType = c.EntityType.ToString(),
                EntityId = c.EntityId,
                EntityTitle = c.EntityTitle,
                EntityKey = c.EntityKey,
                EntityExists = c.EntityExists,
                Author = c.Author,
                CanRevert = c.CanRevert
            }).ToList(),
            TotalPages = result.PageCount,
            CurrentPage = request.Page
        };
    }

    /// <summary>
    /// Gets detailed information about a specific changeset.
    /// </summary>
    [McpServerTool(Name = "get_changeset_details")]
    [Description("Get detailed information about a specific changeset, including what was changed.")]
    public async Task<ChangesetDetailsResult> GetChangesetDetails(
        [Description("Changeset ID (GUID)")] string changesetId)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var id = Guid.Parse(changesetId);
        var details = await changesetsService.GetChangesetDetailsAsync(id);

        return new ChangesetDetailsResult
        {
            Id = details.Id,
            Date = details.Date.DateTime,
            ChangeType = details.ChangeType.ToString(),
            EntityType = details.EntityType.ToString(),
            EntityKey = details.EntityKey,
            EntityId = details.EntityId,
            Author = details.Author,
            CanRevert = details.CanRevert,
            ThumbnailUrl = details.ThumbnailUrl,
            Changes = details.Changes?.Select(c => new ChangeDetails
            {
                Title = c.Title,
                Diff = c.Diff
            }).ToList() ?? []
        };
    }

    /// <summary>
    /// Gets recent activity summary.
    /// </summary>
    [McpServerTool(Name = "get_recent_activity")]
    [Description("Get a summary of recent activity in the wiki.")]
    public async Task<RecentActivityResult> GetRecentActivity(
        [Description("Number of recent changesets to include (default: 20, max: 100)")] int count = 20)
    {
        await authService.RequireRoleAsync(UserRole.User);

        count = Math.Min(count, 100);

        var changes = await db.Changes
            .AsNoTracking()
            .Include(x => x.Author)
            .Include(x => x.EditedPage)
            .Include(x => x.EditedMedia)
            .Include(x => x.EditedRelation).ThenInclude(r => r.Source)
            .Include(x => x.EditedRelation).ThenInclude(r => r.Destination)
            .OrderByDescending(x => x.Date)
            .Take(count)
            .ToListAsync();

        var activityItems = changes.Select(c => new ActivityItem
        {
            Date = c.Date.DateTime,
            ChangeType = c.ChangeType.ToString(),
            EntityType = c.EntityType.ToString(),
            EntityTitle = GetEntityTitle(c),
            EntityKey = c.EditedPage?.Key ?? c.EditedMedia?.Key,
            EntityId = c.EditedPageId ?? c.EditedMediaId ?? c.EditedRelationId,
            Author = c.Author != null ? $"{c.Author.FirstName} {c.Author.LastName}".Trim() : "Unknown",
            AuthorId = c.Author?.Id
        }).ToList();

        // Calculate summary
        var summary = new ActivitySummary
        {
            TotalChanges = activityItems.Count,
            ChangesByType = activityItems.GroupBy(x => x.ChangeType).ToDictionary(g => g.Key, g => g.Count()),
            ChangesByEntityType = activityItems.GroupBy(x => x.EntityType).ToDictionary(g => g.Key, g => g.Count()),
            TopContributors = activityItems
                .Where(x => !string.IsNullOrEmpty(x.AuthorId))
                .GroupBy(x => new { x.AuthorId, x.Author })
                .Select(g => new ContributorSummary
                {
                    UserId = g.Key.AuthorId,
                    Name = g.Key.Author,
                    ChangeCount = g.Count()
                })
                .OrderByDescending(x => x.ChangeCount)
                .Take(5)
                .ToList()
        };

        return new RecentActivityResult
        {
            Items = activityItems,
            Summary = summary
        };
    }

    /// <summary>
    /// Gets wiki statistics.
    /// </summary>
    [McpServerTool(Name = "get_wiki_stats")]
    [Description("Get overall statistics about the wiki content.")]
    public async Task<WikiStatsResult> GetWikiStats()
    {
        await authService.RequireRoleAsync(UserRole.User);

        var pageStats = await db.Pages
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        var mediaStats = await db.Media
            .Where(x => !x.IsDeleted)
            .GroupBy(x => x.Type)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync();

        return new WikiStatsResult
        {
            TotalPages = await db.Pages.CountAsync(x => !x.IsDeleted),
            PagesByType = pageStats.ToDictionary(x => x.Type.ToString(), x => x.Count),
            TotalMedia = await db.Media.CountAsync(x => !x.IsDeleted),
            MediaByType = mediaStats.ToDictionary(x => x.Type.ToString(), x => x.Count),
            TotalRelations = await db.Relations.CountAsync(x => !x.IsComplementary && !x.IsDeleted),
            TotalUsers = await db.Users.CountAsync(x => x.IsValidated),
            TotalChangesets = await db.Changes.CountAsync()
        };
    }

    private string GetEntityTitle(Changeset c)
    {
        if (c.EditedPage != null)
            return c.EditedPage.Title;
        if (c.EditedMedia != null)
            return c.EditedMedia.Title;
        if (c.EditedRelation != null)
            return $"{c.EditedRelation.Source?.Title ?? "?"} → {c.EditedRelation.Destination?.Title ?? "?"}";
        return "Unknown";
    }
}

#region Result Types

public class ListChangesetsResult
{
    public List<ChangesetListItem> Changesets { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class ChangesetListItem
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string ChangeType { get; set; }
    public string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string EntityTitle { get; set; }
    public string EntityKey { get; set; }
    public bool EntityExists { get; set; }
    public string Author { get; set; }
    public bool CanRevert { get; set; }
}

public class ChangesetDetailsResult
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string ChangeType { get; set; }
    public string EntityType { get; set; }
    public string EntityKey { get; set; }
    public Guid EntityId { get; set; }
    public string Author { get; set; }
    public bool CanRevert { get; set; }
    public string ThumbnailUrl { get; set; }
    public List<ChangeDetails> Changes { get; set; }
}

public class ChangeDetails
{
    public string Title { get; set; }
    public string Diff { get; set; }
}

public class RecentActivityResult
{
    public List<ActivityItem> Items { get; set; }
    public ActivitySummary Summary { get; set; }
}

public class ActivityItem
{
    public DateTime Date { get; set; }
    public string ChangeType { get; set; }
    public string EntityType { get; set; }
    public string EntityTitle { get; set; }
    public string EntityKey { get; set; }
    public Guid? EntityId { get; set; }
    public string Author { get; set; }
    public string AuthorId { get; set; }
}

public class ActivitySummary
{
    public int TotalChanges { get; set; }
    public Dictionary<string, int> ChangesByType { get; set; }
    public Dictionary<string, int> ChangesByEntityType { get; set; }
    public List<ContributorSummary> TopContributors { get; set; }
}

public class ContributorSummary
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public int ChangeCount { get; set; }
}

public class WikiStatsResult
{
    public int TotalPages { get; set; }
    public Dictionary<string, int> PagesByType { get; set; }
    public int TotalMedia { get; set; }
    public Dictionary<string, int> MediaByType { get; set; }
    public int TotalRelations { get; set; }
    public int TotalUsers { get; set; }
    public int TotalChangesets { get; set; }
}

#endregion
