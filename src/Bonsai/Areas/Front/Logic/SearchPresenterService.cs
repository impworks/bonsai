using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bonsai.Areas.Front.ViewModels.Page;
using Bonsai.Areas.Front.ViewModels.Search;
using Bonsai.Code.Services.Search;
using Bonsai.Data;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Areas.Front.Logic;

/// <summary>
/// The presenter for search results.
/// </summary>
public class SearchPresenterService
{
    public SearchPresenterService(AppDbContext db, ISearchEngine search, IMapper mapper)
    {
        _db = db;
        _search = search;
        _mapper = mapper;
    }

    private readonly AppDbContext _db;
    private readonly ISearchEngine _search;
    private readonly IMapper _mapper;

    private const int MIN_QUERY_LENGTH = 3;

    /// <summary>
    /// Returns the exact match if one exists.
    /// </summary>
    public async Task<PageTitleVM> FindExactAsync(string query)
    {
        return await _db.Pages
                        .Where(x => x.IsDeleted == false && x.Aliases.Any(y => y.Title == query))
                        .ProjectToType<PageTitleVM>(_mapper.Config)
                        .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Find pages matching the query.
    /// </summary>
    public async Task<IReadOnlyList<SearchResultVM>> SearchAsync(string query, int page = 0)
    {
        var q = (query ?? "").Trim();
        if(q.Length < MIN_QUERY_LENGTH)
            return Array.Empty<SearchResultVM>();

        var matches = await _search.SearchAsync(q, page);
        var ids = matches.Select(x => x.Id);

        var details = await _db.Pages
                               .Where(x => ids.Contains(x.Id) && x.IsDeleted == false)
                               .Select(x => new { x.Id, x.MainPhoto.FilePath, x.LastUpdateDate, PageType = x.Type })
                               .ToDictionaryAsync(x => x.Id, x => x);

        var results = matches
                      .Where(x => details.ContainsKey(x.Id))
                      .Select(x => new SearchResultVM
                      {
                          Id = x.Id,
                          Key = x.Key,
                          Title = x.Title,
                          HighlightedTitle = x.HighlightedTitle,
                          Type = details[x.Id].PageType,
                          DescriptionExcerpt = x.HighlightedDescription,
                          MainPhotoPath = details[x.Id].FilePath,
                          LastUpdateDate = details[x.Id].LastUpdateDate,
                      });

        return results.ToList();
    }

    /// <summary>
    /// Shows autocomplete suggestions for the search box.
    /// </summary>
    public async Task<IReadOnlyList<PageTitleVM>> SuggestAsync(string query)
    {
        var q = (query ?? "").Trim();
        if(q.Length < MIN_QUERY_LENGTH)
            return Array.Empty<PageTitleVM>();

        var results = await _search.SuggestAsync(q, maxCount: 10);

        return results.Select(x => new SearchResultVM
                      {
                          Id = x.Id,
                          Title = x.Title,
                          HighlightedTitle = x.HighlightedTitle,
                          Key = x.Key,
                          Type = x.PageType
                      })
                      .ToList();
    }
}