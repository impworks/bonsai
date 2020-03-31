namespace Bonsai.Areas.Admin.ViewModels.Common
{
    /// <summary>
    /// Info about a single link used somewhere in a message.
    /// </summary>
    public class LinkVM
    {
        /// <summary>
        /// Displayed title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// URL of the link.
        /// </summary>
        public string Url { get; set; }
    }
}
