namespace Bonsai.Areas.Admin.ViewModels.Tree
{
    /// <summary>
    /// A person displayed on the tree.
    /// </summary>
    public class TreePersonVM
    {
        /// <summary>
        /// ID of the person.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Person's full name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Maiden name of the person (if any).
        /// </summary>
        public string MaidenName { get; set; }

        /// <summary>
        /// Gender flag.
        /// </summary>
        public bool IsMale { get; set; }

        /// <summary>
        /// Date of birth.
        /// </summary>
        public string Birth { get; set; }

        /// <summary>
        /// Date of death.
        /// </summary>
        public string Death { get; set; }

        /// <summary>
        /// Flag indicating that the person is dead (even if the date is unknown).
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// URL to the photo.
        /// </summary>
        public string Photo { get; set; }

        /// <summary>
        /// URL to the page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// ID of the parent relation.
        /// </summary>
        public string Parents { get; set; }
    }
}
