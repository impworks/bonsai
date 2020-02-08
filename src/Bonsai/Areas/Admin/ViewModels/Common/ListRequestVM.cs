namespace Bonsai.Areas.Admin.ViewModels.Common
{
    /// <summary>
    /// Base request for entity list filtering.
    /// </summary>
    public class ListRequestVM
    {
        /// <summary>
        /// Search query for page names.
        /// </summary>
        public string SearchQuery { get; set; }

        /// <summary>
        /// Ordering field.
        /// </summary>
        public string OrderBy { get; set; }

        /// <summary>
        /// Ordering direction.
        /// </summary>
        public bool? OrderDescending { get; set; }

        /// <summary>
        /// Current page (0-based).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Checks if the request is empty.
        /// </summary>
        public virtual bool IsEmpty()
        {
            return string.IsNullOrEmpty(SearchQuery)
                   && Page == 0;
        }

        /// <summary>
        /// Creates a clone of this object.
        /// </summary>
        public static T Clone<T>(T request) where T: ListRequestVM => (T) request.MemberwiseClone();
    }
}
