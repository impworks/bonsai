using System;
using Bonsai.Code.Infrastructure;
using Bonsai.Data.Models;
using Mapster;

namespace Bonsai.Areas.Front.ViewModels.Page;

/// <summary>
/// Base view model for all page sections.
/// </summary>
public class PageTitleVM: IMapped
{
    /// <summary>
    /// Surrogate ID of the page.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Title of the page (displayed in the header).
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Key of the page (url-friendly version of the title).
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Type of the entity described by this page.
    /// </summary>
    public PageType Type { get; set; }

    public virtual void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<Data.Models.Page, PageTitleVM>()
              .Map(x => x.Id, x => x.Id)
              .Map(x => x.Title, x => x.Title)
              .Map(x => x.Key, x => x.Key)
              .Map(x => x.Type, x => x.Type);
    }
}