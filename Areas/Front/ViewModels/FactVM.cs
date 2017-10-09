namespace Bonsai.Areas.Front.ViewModels
{
    /// <summary>
    /// Information about a single fact.
    /// </summary>
    public class FactVM
    {
        /// <summary>
        /// Readable fact title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Path to the template to display contents of this fact.
        /// </summary>
        public string TemplatePath { get; set; }

        /// <summary>
        /// Fact data.
        /// </summary>
        public object Data { get; set; }
    }
}
