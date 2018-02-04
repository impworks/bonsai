﻿using Bonsai.Code.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

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
        /// Renders the tag.
        /// </summary>
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var image = StringHelper.Coalesce(Image, FallbackImage, "~/assets/img/unknown-user.png");
            var path = _url.Content(image);

            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.Add("class", "page-image");
            output.Attributes.Add("style", $"background-image: url({path})");
        }
    }
}