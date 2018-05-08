using Bonsai.Code.Utils;
using Bonsai.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Linq;

namespace Bonsai.Areas.Front.TagHelpers
{
    /// <summary>
    /// Displays a photograph for a page.
    /// </summary>
    [HtmlTargetElement("page-image", Attributes = "image")]
    public class PageImageTagHelper: TagHelper
    {
        public PageImageTagHelper(IUrlHelper url)
        {
            _url = url;
        }

        private readonly IUrlHelper _url;

        /// <summary>
        /// Path to the image.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Path to a fallback image (if the original one is not set).
        /// </summary>
        public string FallbackImage { get; set; }

        /// <summary>
        /// Type of the entity.
        /// </summary>
        public PageType? Type { get; set; }

        /// <summary>
        /// Renders the tag.
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var type = (Type ?? PageType.Person).ToString().ToLower();
            var image = StringHelper.Coalesce(Image, FallbackImage, $"~/assets/img/unknown-{type}.svg");

            var path = _url.Content(image);
            var className = "image";
            
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var existingClass = output.Attributes.FirstOrDefault(x => x.Name == "class")?.Value;
            if (existingClass == null)
                output.Attributes.Add("class", className);
            else
                output.Attributes.SetAttribute("class", $"{className} {existingClass}");

            output.Attributes.Add("style", $"background-image: url({path})");
        }
    }
}
