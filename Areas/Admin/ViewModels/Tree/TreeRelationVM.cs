namespace Bonsai.Areas.Admin.ViewModels.Tree
{
    /// <summary>
    /// Spouse relation.
    /// </summary>
    public class TreeRelationVM
    {
        /// <summary>
        /// Relation's unique ID.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// ID of the first related person.
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// ID of the second related person.
        /// </summary>
        public string To { get; set; }
    }
}
