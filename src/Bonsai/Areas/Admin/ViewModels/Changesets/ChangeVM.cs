namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Details about a particular property.
    /// </summary>
    public class ChangeVM
    {
        /// <summary>
        /// Title of the property.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Difference in property values.
        /// </summary>
        public string Diff { get; set; }
    }
}
