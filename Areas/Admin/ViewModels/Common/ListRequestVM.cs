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
        public bool OrderDescending { get; set; }

        /// <summary>
        /// Current page (0-based).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Creates a clone of this object.
        /// </summary>
        public static T Clone<T>(T obj) where T: ListRequestVM => (T) obj.MemberwiseClone();
    }
}
