using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Code.Utils;
using Bonsai.Data;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bonsai.Code.Services
{
    /// <summary>
    /// Markdown to HTML compilation service.
    /// </summary>
    public class MarkdownService
    {
        public MarkdownService(AppDbContext context, IUrlHelper urlHelper)
        {
            _db = context;
            _url = urlHelper;

            _pipeline = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseAutoIdentifiers()
                .UseEmphasisExtras()
                .UseBootstrap()
                .Build();
        }

        private readonly MarkdownPipeline _pipeline;
        private readonly AppDbContext _db;
        private readonly IUrlHelper _url;

        private static readonly Regex LinkRegex = new Regex(@"\[\[(?<key>[^\[|]+)(\|(?<label>[^\[]+))?\]\]");

        /// <summary>
        /// Renders the markdown text to HTML.
        /// </summary>
        public async Task<string> CompileAsync(string markdown)
        {
            var body = Markdown.ToHtml(markdown, _pipeline);
            body = await ProcessLinksAsync(body).ConfigureAwait(false);

            return body;
        }

        /// <summary>
        /// Compiles [[...]] links to HTML links.
        /// </summary>
        private async Task<string> ProcessLinksAsync(string html)
        {
            var keys = LinkRegex.Matches(html)
                                .Select(x => x.Groups["key"].Value)
                                .ToDictionary(x => x, PageHelper.EncodeTitle);

            if (!keys.Any())
                return html;

            var existingPages = await _db.Pages
                                         .Where(x => keys.Values.Contains(x.Key))
                                         .ToDictionaryAsync(x => x.Key, x => true)
                                         .ConfigureAwait(false);

            return LinkRegex.Replace(html, m =>
            {
                var rawKey = m.Groups["key"].Value;
                var key = keys[rawKey];
                var title = m.Groups["label"].Success ? m.Groups["label"].Value : rawKey;

                // todo: encoding
                if (existingPages.ContainsKey(key))
                    return $@"<a href=""{_url.Action("Description", "Page", new {key = key})}"" class=""link"">{title}</a>";

                return $@"<span class=""missing-link"" title=""Страница не найдена: {rawKey}"">{title}</span>";
            });
        }
    }
}
