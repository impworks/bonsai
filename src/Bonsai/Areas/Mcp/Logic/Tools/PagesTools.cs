using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Admin.Logic;
using Bonsai.Areas.Admin.Logic.Tree;
using Bonsai.Areas.Admin.ViewModels.Pages;
using Bonsai.Areas.Front.Logic;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Code.Services.Jobs;
using Bonsai.Code.Services.Search;
using Bonsai.Data;
using Bonsai.Data.Models;
using ModelContextProtocol.Server;

namespace Bonsai.Areas.Mcp.Logic.Tools;

/// <summary>
/// MCP tools for managing wiki pages.
/// </summary>
[McpServerToolType]
public class PagesTools(
    SearchPresenterService searchService,
    PagePresenterService pagePresenterService,
    PagesManagerService pagesManagerService,
    McpToolAuthorizationService authService,
    McpUserContext userContext,
    AppDbContext db,
    ISearchEngine search,
    IBackgroundJobService jobs)
{
    /// <summary>
    /// Searches for pages using full-text search.
    /// </summary>
    [McpServerTool(Name = "search_pages")]
    [Description("Search for wiki pages using full-text search. Returns matching pages with highlighted excerpts.")]
    public async Task<SearchPagesResult> SearchPages(
        [Description("The search query (minimum 3 characters)")] string query,
        [Description("Page number for pagination (0-based, default: 0)")] int page = 0)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var results = await searchService.SearchAsync(query, page);

        return new SearchPagesResult
        {
            Results = results.Select(r => new PageSearchHit
            {
                Id = r.Id,
                Key = r.Key,
                Title = r.Title,
                HighlightedTitle = r.HighlightedTitle,
                Type = r.Type.ToString(),
                DescriptionExcerpt = r.DescriptionExcerpt
            }).ToList()
        };
    }

    /// <summary>
    /// Gets autocomplete suggestions for page search.
    /// </summary>
    [McpServerTool(Name = "suggest_pages")]
    [Description("Get autocomplete suggestions for page titles. Useful for finding pages by partial name.")]
    public async Task<SuggestPagesResult> SuggestPages(
        [Description("The partial title to search for (minimum 3 characters)")] string query)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var results = await searchService.SuggestAsync(query);

        return new SuggestPagesResult
        {
            Suggestions = results.Select(r => new PageSuggestion
            {
                Id = r.Id ?? Guid.Empty,
                Key = r.Key,
                Title = r.Title,
                Type = r.Type.ToString()
            }).ToList()
        };
    }

    /// <summary>
    /// Reads a page's content and metadata.
    /// </summary>
    [McpServerTool(Name = "read_page")]
    [Description("Read a wiki page by its key (URL identifier). Returns the page description and info block.")]
    public async Task<ReadPageResult> ReadPage(
        [Description("The page key (URL identifier, e.g., 'John_Doe')")] string key)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var description = await pagePresenterService.GetPageDescriptionAsync(key);
        var infoBlock = await pagePresenterService.GetPageInfoBlockAsync(key);

        return new ReadPageResult
        {
            Id = description.Id ?? Guid.Empty,
            Key = description.Key,
            Title = description.Title,
            Type = description.Type.ToString(),
            Description = description.Description,
            PhotoPath = infoBlock.Photo?.ThumbnailUrl,
            Facts = infoBlock.Facts?.SelectMany(g => g.Facts.Select(f => new FactInfo
            {
                GroupId = g.Definition.Id,
                GroupTitle = g.Definition.Title,
                FactId = f.Definition.Id,
                FactTitle = f.Definition.Title,
                Value = f.ShortTitle ?? f.ToString()
            })).ToList() ?? [],
            Relations = infoBlock.RelationGroups?.SelectMany(c =>
                c.Groups.SelectMany(g =>
                    g.Relations.SelectMany(r =>
                        r.Pages.Select(p => new RelationInfo
                        {
                            GroupName = c.Title,
                            RelationType = r.Title,
                            PersonId = p.Id ?? Guid.Empty,
                            PersonKey = p.Key,
                            PersonName = p.Title,
                            Duration = p.Duration?.ReadableRange
                        })
                    )
                )
            ).ToList() ?? []
        };
    }

    /// <summary>
    /// Lists pages with filters.
    /// </summary>
    [McpServerTool(Name = "list_pages")]
    [Description("List wiki pages with optional filters. Supports filtering by type and searching by title.")]
    public async Task<ListPagesResult> ListPages(
        [Description("Filter by page types (comma-separated: Person, Pet, Location, Event, Other)")] string types = null,
        [Description("Search query to filter by title")] string searchQuery = null,
        [Description("Field to order by (Title, LastUpdateDate, CreationDate, CompletenessScore)")] string orderBy = "Title",
        [Description("Order descending (default: false)")] bool orderDescending = false,
        [Description("Page number for pagination (0-based, default: 0)")] int page = 0)
    {
        await authService.RequireRoleAsync(UserRole.User);

        var typesList = string.IsNullOrEmpty(types)
            ? null
            : types.Split(',')
                   .Select(t => Enum.TryParse<PageType>(t.Trim(), true, out var pt) ? pt : (PageType?)null)
                   .Where(t => t.HasValue)
                   .Select(t => t.Value)
                   .ToArray();

        var request = new PagesListRequestVM
        {
            Types = typesList,
            SearchQuery = searchQuery,
            OrderBy = orderBy,
            OrderDescending = orderDescending,
            Page = page
        };

        var result = await pagesManagerService.GetPagesAsync(request);

        return new ListPagesResult
        {
            Pages = result.Items.Select(p => new PageListItem
            {
                Id = p.Id,
                Key = p.Key,
                Title = p.Title,
                Type = p.Type.ToString(),
                LastUpdateDate = p.LastUpdateDate,
                CompletenessScore = p.CompletenessScore
            }).ToList(),
            TotalPages = result.PageCount,
            CurrentPage = request.Page
        };
    }

    /// <summary>
    /// Creates a new page.
    /// </summary>
    [McpServerTool(Name = "create_page")]
    [Description("Create a new wiki page. Requires Editor role.")]
    public async Task<CreatePageResult> CreatePage(
        [Description("Page title")] string title,
        [Description("Page type (Person, Pet, Location, Event, Other)")] string type,
        [Description("Page description in Markdown format")] string description,
        [Description("Page facts as JSON (optional, format depends on page type)")] string facts = null,
        [Description("Comma-separated list of title aliases (optional)")] string aliases = null)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var pageType = Enum.Parse<PageType>(type, ignoreCase: true);

        var vm = new PageEditorVM
        {
            Title = title,
            Type = pageType,
            Description = description,
            Facts = facts,
            Aliases = aliases != null ? System.Text.Json.JsonSerializer.Serialize(aliases.Split(',').Select(a => a.Trim()).ToList()) : null
        };

        var page = await pagesManagerService.CreateAsync(vm, userContext.Principal);
        await db.SaveChangesAsync();

        await search.AddPageAsync(page);
        await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());

        return new CreatePageResult
        {
            Id = page.Id,
            Key = page.Key,
            Title = page.Title
        };
    }

    /// <summary>
    /// Updates an existing page.
    /// </summary>
    [McpServerTool(Name = "update_page")]
    [Description("Update an existing wiki page. Requires Editor role.")]
    public async Task<UpdatePageResult> UpdatePage(
        [Description("Page ID (GUID)")] string pageId,
        [Description("New page title (optional)")] string title = null,
        [Description("New page description in Markdown (optional)")] string description = null,
        [Description("New page facts as JSON (optional)")] string facts = null,
        [Description("New comma-separated list of title aliases (optional)")] string aliases = null)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(pageId);
        var current = await pagesManagerService.RequestUpdateAsync(id, userContext.Principal, force: true);

        if (title != null) current.Title = title;
        if (description != null) current.Description = description;
        if (facts != null) current.Facts = facts;
        if (aliases != null) current.Aliases = System.Text.Json.JsonSerializer.Serialize(aliases.Split(',').Select(a => a.Trim()).ToList());

        var page = await pagesManagerService.UpdateAsync(current, userContext.Principal);
        await db.SaveChangesAsync();

        await search.AddPageAsync(page);
        await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());

        return new UpdatePageResult
        {
            Id = page.Id,
            Key = page.Key,
            Title = page.Title,
            Success = true
        };
    }

    /// <summary>
    /// Deletes a page (soft delete).
    /// </summary>
    [McpServerTool(Name = "delete_page")]
    [Description("Delete a wiki page (soft delete, can be restored). Requires Editor role.")]
    public async Task<DeletePageResult> DeletePage(
        [Description("Page ID (GUID)")] string pageId)
    {
        await authService.RequireRoleAsync(UserRole.Editor);

        var id = Guid.Parse(pageId);
        var page = await pagesManagerService.RemoveAsync(id, userContext.Principal);
        await db.SaveChangesAsync();

        await search.RemovePageAsync(id);
        await jobs.RunAsync(JobBuilder.For<TreeLayoutJob>().SupersedeAll());

        return new DeletePageResult
        {
            Id = page.Id,
            Title = page.Title,
            Success = true
        };
    }

    /// <summary>
    /// Gets the last updated pages.
    /// </summary>
    [McpServerTool(Name = "get_recent_pages")]
    [Description("Get the most recently updated wiki pages.")]
    public async Task<RecentPagesResult> GetRecentPages(
        [Description("Number of pages to return (default: 10, max: 50)")] int count = 10)
    {
        await authService.RequireRoleAsync(UserRole.User);

        count = Math.Min(count, 50);
        var pages = await pagePresenterService.GetLastUpdatedPagesAsync(count);

        return new RecentPagesResult
        {
            Pages = pages.Select(p => new PageListItem
            {
                Id = p.Id,
                Key = p.Key,
                Title = p.Title,
                Type = p.Type.ToString(),
                LastUpdateDate = p.LastUpdateDate
            }).ToList()
        };
    }
}

#region Result Types

public class SearchPagesResult
{
    public List<PageSearchHit> Results { get; set; }
}

public class PageSearchHit
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public string HighlightedTitle { get; set; }
    public string Type { get; set; }
    public string DescriptionExcerpt { get; set; }
}

public class SuggestPagesResult
{
    public List<PageSuggestion> Suggestions { get; set; }
}

public class PageSuggestion
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
}

public class ReadPageResult
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string PhotoPath { get; set; }
    public List<FactInfo> Facts { get; set; }
    public List<RelationInfo> Relations { get; set; }
}

public class FactInfo
{
    public string GroupId { get; set; }
    public string GroupTitle { get; set; }
    public string FactId { get; set; }
    public string FactTitle { get; set; }
    public string Value { get; set; }
}

public class RelationInfo
{
    public string GroupName { get; set; }
    public string RelationType { get; set; }
    public Guid PersonId { get; set; }
    public string PersonKey { get; set; }
    public string PersonName { get; set; }
    public string Duration { get; set; }
}

public class ListPagesResult
{
    public List<PageListItem> Pages { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
}

public class PageListItem
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
    public DateTimeOffset LastUpdateDate { get; set; }
    public double? CompletenessScore { get; set; }
}

public class CreatePageResult
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
}

public class UpdatePageResult
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public string Title { get; set; }
    public bool Success { get; set; }
}

public class DeletePageResult
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public bool Success { get; set; }
}

public class RecentPagesResult
{
    public List<PageListItem> Pages { get; set; }
}

#endregion
