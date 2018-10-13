using System;

namespace Bonsai.Areas.Admin.ViewModels.Pages
{
    /// <summary>
    /// Information about a saved page draft.
    /// </summary>
    public class PageDraftInfoVM
    {
        /// <summary>
        /// Draft's timestamp.
        /// </summary>
        public DateTimeOffset LastUpdateDate { get; set; }
    }
}
