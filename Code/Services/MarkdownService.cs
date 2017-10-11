using Markdig;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Markdown to HTML compilation service.
    /// </summary>
    public class MarkdownService
    {
        public MarkdownService()
        {
            _pipeline = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseAutoIdentifiers()
                .UseEmphasisExtras()
                .UseBootstrap()
                .Build();
        }

        private readonly MarkdownPipeline _pipeline;

        /// <summary>
        /// Renders the markdown text to HTML.
        /// </summary>
        public string Compile(string markdown)
        {
            return Markdown.ToHtml(markdown, _pipeline);
        }
    }
}
