namespace Bonsai.Areas.Admin.ViewModels.Changesets
{
    /// <summary>
    /// Extra data for the list of changesets.
    /// </summary>
    public class ChangesetsListDataVM
    {
        /// <summary>
        /// Title of the filter-by entity.
        /// </summary>
        public string EntityTitle { get; set; }

        /// <summary>
        /// Title of the filter-by user.
        /// </summary>
        public string UserTitle { get; set; }
    }
}
