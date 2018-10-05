using System.Collections.Generic;

namespace Bonsai.Areas.Admin.ViewModels.Common
{
    /// <summary>
    /// Base VM for all result list views.
    /// </summary>
    public class ListResultVM<TRequest, TEntity>
        where TRequest: ListRequestVM
    {
        /// <summary>
        /// Current search query.
        /// </summary>
        public TRequest Request { get; set; }

        /// <summary>
        /// List of pages.
        /// </summary>
        public IReadOnlyList<TEntity> Items { get; set; }

        /// <summary>
        /// Number of pages of data.
        /// </summary>
        public int PageCount { get; set; }
    }
}
