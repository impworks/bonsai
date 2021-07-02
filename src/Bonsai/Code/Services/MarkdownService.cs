using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
using Bonsai.Code.Utils.Helpers;
using Bonsai.Data;
using JetBrains.Annotations;
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

            _render = new MarkdownPipelineBuilder()
                .UseAutoLinks()
                .UseAutoIdentifiers()
                .UseEmphasisExtras()
                .UseBootstrap()
                .Build();
        }

        private readonly MarkdownPipeline _render;
        private readonly AppDbContext _db;
        private readonly IUrlHelper _url;

        private static readonly Regex MediaRegex = Compile(@"(?<tag><p>)?\[\[media:(?<key>[^\[|]+)(\|(?<options>[^\]]+))?\]\](?(tag)</p>|)");
        private static readonly Regex LinkRegex = Compile(@"\[\[(?<key>[^\[|]+)(\|(?<label>[^\]]+))?\]\]");
        private static readonly Regex MarkupRegex = Compile(@"[*#|=_]+");

        private static string[] MediaSizeClasses = {"large", "medium", "small"};
        private static string[] MediaAlignmentClasses = {"left", "right"};

        /// <summary>
        /// Renders the markdown text to HTML.
        /// </summary>
        public async Task<string> CompileAsync(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return null;

            var body = Markdown.ToHtml(markdown, _render);

            body = await ProcessMediaAsync(body);
            body = await ProcessLinksAsync(body);

            return body;
        }

        /// <summary>
        /// Removes all markdown text from the source.
        /// </summary>
        public static string Strip(string markdown)
        {
            var rendered = markdown;

            rendered = MediaRegex.Replace(rendered, "");
            rendered = LinkRegex.Replace(rendered, me => me.Groups["label"]?.Value ?? me.Groups["key"]?.Value ?? "");
            rendered = MarkupRegex.Replace(rendered, "");

            return rendered;
        }

        /// <summary>
        /// Compiles [[media:...]] links to images.
        /// </summary>
        private async Task<string> ProcessMediaAsync(string html)
        {
            var keys = MediaRegex.Matches(html)
                                 .Select(x => x.Groups["key"].Value)
                                 .Distinct()
                                 .ToDictionary(x => x, PageHelper.GetMediaId);

            if (!keys.Any())
                return html;

            var existingMedia = await _db.Media
                                         .AsNoTracking()
                                         .Where(x => keys.Values.Contains(x.Id))
                                         .ToDictionaryAsync(x => x.Key, x => x.FilePath);

            string Wrapper(string classes, string body) => $@"<div class=""media-wrapper-inline {classes}"">{body}</div>";

            return MediaRegex.Replace(html, m =>
            {
                var key = m.Groups["key"].Value;
                var args = m.Groups["options"].Value?.Split('|');
                var details = GetMediaDetails(args);

                if (!existingMedia.TryGetValue(key, out var rawPath))
                    return Wrapper(details.Classes + " error", $"<p class='caption'>Медиа-файл <span class='break-word'>'{key}'</span> не найден.</p>");

                if(details.Error)
                    return Wrapper("right error", $"<p class='caption'>{details.Descr}</p>");

                var link = _url.Action("ViewMedia", "Media", new { area = "Front", key = key});
                var path = _url.Content(MediaPresenterService.GetSizedMediaPath(rawPath, MediaSize.Small));

                var body = $@"
                    <a href=""{link}"" class=""media-thumb-link"" data-media=""{key}"">
                        <img src=""{path}"" />
                    </a>
                    <p class='caption'>{details.Descr}</p>
                ";

                return Wrapper(details.Classes, body);
            });
        }

        /// <summary>
        /// Compiles [[...]] links to HTML links.
        /// </summary>
        private async Task<string> ProcessLinksAsync(string html)
        {
            var keys = LinkRegex.Matches(html)
                                .Select(x => x.Groups["key"].Value)
                                .Distinct()
                                .ToDictionary(x => x, x => PageHelper.EncodeTitle(x).ToLowerInvariant());

            if (!keys.Any())
                return html;

            var existingPages = await _db.PageAliases
                                         .AsNoTracking()
                                         .Where(x => keys.Values.Contains(x.Key))
                                         .ToDictionaryAsync(x => x.Key, x => PageHelper.EncodeTitle(x.Title));

            return LinkRegex.Replace(html, m =>
            {
                var rawKey = m.Groups["key"].Value;
                var lowerKey = keys[rawKey];
                var title = m.Groups["label"].Success ? m.Groups["label"].Value : rawKey;

                if (existingPages.TryGetValue(lowerKey, out var canonKey))
                    return $@"<a href=""{_url.Action("Description", "Page", new { area = "Front", key = canonKey })}"" class=""link"">{title}</a>";

                return $@"<span class=""link-missing"" title=""Страница не найдена: {rawKey}"">{title}</span>";
            });
        }

        /// <summary>
        /// Parses the media description.
        /// </summary>
        private (string Classes, string Descr, bool Error) GetMediaDetails(string[] args)
        {
            (string, string, bool) Error(string msg) => (null, msg, true);

            string sizeClass = null;
            string alignClass = null;
            string descr = null;

            if (args != null)
            {
                foreach (var item in args)
                {
                    if (item.StartsWith("size:"))
                    {
                        var size = item.Substring("size:".Length);
                        if (!MediaSizeClasses.Contains(size))
                            return Error("Неизвестный размер медиа-файла.");

                        if (sizeClass != null)
                            return Error("Размер указан более одного раза.");

                        sizeClass = size;
                        continue;
                    }

                    if (item.StartsWith("align:"))
                    {
                        var align = item.Substring("align:".Length);
                        if (!MediaAlignmentClasses.Contains(align))
                            return Error("Неизвестное расположение медиа-файла.");

                        if (alignClass != null)
                            return Error("Расположение указано более одного раза.");

                        alignClass = align;
                        continue;
                    }

                    if (descr != null)
                        return Error("Описание указано более одного раза.");

                    descr = item;
                }
            }

            var classes = (sizeClass ?? MediaSizeClasses[0]) + " " + (alignClass ?? MediaAlignmentClasses[0]);
            return (classes, descr, false);
        }

        /// <summary>
        /// Compiles a regular expression with certain options.
        /// </summary>
        private static Regex Compile([RegexPattern] string pattern)
        {
            var options = RegexOptions.Compiled
                          | RegexOptions.IgnoreCase
                          | RegexOptions.CultureInvariant
                          | RegexOptions.ExplicitCapture;
            return new Regex(pattern, options);
        }
    }
}
