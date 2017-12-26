namespace Bonsai.Areas.Front.Logic.Facts
{
    /// <summary>
    /// Useful methods for fact templates.
    /// </summary>
    public static class FactTemplateExtensions
    {
        /// <summary>
        /// Returns the path for the fact's viewer template.
        /// </summary>
        public static string GetViewTemplatePath(this FactTemplate template)
        {
            return $"~/Areas/Front/Views/Page/Facts/{template}.cshtml";
        }
    }
}
