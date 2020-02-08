using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Bonsai.Areas.Admin.TagHelpers
{
    /// <summary>
    /// Special validation error renderer for the fact list.
    /// </summary>
    [HtmlTargetElement(Attributes = "validation-list-for")]
    public class ValidationListTagHelper: TagHelper
    {
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }
        
        [HtmlAttributeName("validation-list-caption")]
        public string Caption { get; set; }
        
        [HtmlAttributeName("validation-list-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (For == null)
                return;

            var model = ViewContext.ViewData.ModelState;
            if (!model.TryGetValue(For.Name, out var state) || state.ValidationState != ModelValidationState.Invalid)
            {
                output.SuppressOutput();
                return;
            }

            var ul = new TagBuilder("ul");
            ul.AddCssClass("mb-0");
            foreach (var error in state.Errors)
            {
                var li = new TagBuilder("li");
                li.InnerHtml.Append(error.ErrorMessage);
                ul.InnerHtml.AppendHtml(li);
            }
            
            output.Content.AppendHtml($"<div>{Caption}:</div>");
            output.Content.AppendHtml(ul);
        }
    }
}
