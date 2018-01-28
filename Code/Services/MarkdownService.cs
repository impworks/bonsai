using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bonsai.Areas.Front.Logic;
using Bonsai.Code.DomainModel.Media;
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

        private static readonly Regex MediaRegex = new Regex(@"\[\[media:(?<key>[^\[|]+)(\|(?<options>[^\]]+))?\]\]");
        private static readonly Regex LinkRegex = new Regex(@"\[\[(?<key>[^\[|]+)(\|(?<label>[^\]]+))?\]\]");

        private static string[] MediaSizeClasses = {"large", "medium", "small"};
        private static string[] MediaAlignmentClasses = {"left", "right"};

        /// <summary>
        /// Renders the markdown text to HTML.
        /// </summary>
        public async Task<string> CompileAsync(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
                return null;

            var body = Markdown.ToHtml(markdown, _pipeline);

            body = await ProcessMediaAsync(body).ConfigureAwait(false);
            body = await ProcessLinksAsync(body).ConfigureAwait(false);

            return body;
        }

        /// <summary>
        /// Compiles [[media:...]] links to images.
        /// </summary>
        private async Task<string> ProcessMediaAsync(string html)
        {
            var keys = MediaRegex.Matches(html)
                                .Select(x => x.Groups["key"].Value)
                                .ToDictionary(x => x, PageHelper.GetMediaId);

            if (!keys.Any())
                return html;

            var existingMedia = await _db.Media
                                         .AsNoTracking()
                                         .Where(x => keys.Values.Contains(x.Id))
                                         .ToDictionaryAsync(x => x.Key, x => x.FilePath)
                                         .ConfigureAwait(false);

            return MediaRegex.Replace(html, m =>
            {
                var key = m.Groups["key"].Value;

                if (!existingMedia.TryGetValue(key, out var rawPath))
                    return $@"<div class=""media-inline-wrapper left error"">Медиа-файл '{key}' не найден.</div>";

                var args = m.Groups["options"].Value?.Split('|');
                var details = GetMediaDetails(args);
                var link = _url.Action("ViewMedia", "Media", new {key = key});
                var path = _url.Content(MediaPresenterService.GetSizedMediaPath(rawPath, MediaSize.Small));

                return $@"
                    <div class=""media-wrapper-inline {details.classes}"">
                        <a href=""{link}"" class=""media-thumb-link"" data-media=""{key}"">
                            <img src=""{path}"" />
                        </a>
                        <span>{details.descr}</span>
                    </div>";
            });
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
                                         .AsNoTracking()
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
                    return $@"<a href=""{_url.Action("Description", "Page", new { key = key })}"" class=""link"">{title}</a>";

                return $@"<span class=""link-missing"" title=""Страница не найдена: {rawKey}"">{title}</span>";
            });
        }

        /// <summary>
        /// Parses the media description.
        /// </summary>
        private (string classes, string descr) GetMediaDetails(string[] args)
        {
            (string, string) Error(string msg) => ("error", msg);

            if (args == null || args.Length == 0)
                return ("left", null);

            string sizeClass = null;
            string alignClass = null;
            string descr = null;

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

            var classes = (sizeClass ?? MediaSizeClasses[0]) + " " + (alignClass ?? MediaAlignmentClasses[0]);
            return (classes, descr);
        }
    }
}
