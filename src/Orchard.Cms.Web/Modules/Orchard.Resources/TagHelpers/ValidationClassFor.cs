using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Orchard.Resources.TagHelpers
{
    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting any HTML element with an <c>asp-validation-for</c>
    /// attribute.
    /// </summary>
    [HtmlTargetElement("*", Attributes = ValidationForAttributeName)]
    public class ValidationMessageTagHelper : TagHelper
    {
        private const string ValidationForAttributeName = "asp-validation-class-for";
        private const string HasValidationErrorClassName = "has-validation-error";

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return -1000;
            }
        }

        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Name to be validated on the current model.
        /// </summary>
        [HtmlAttributeName(ValidationForAttributeName)]
        public ModelExpression For { get; set; }

        /// <inheritdoc />
        /// <remarks>Does nothing if <see cref="For"/> is <c>null</c>.</remarks>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (For != null)
            {

                if (ViewContext.ModelState[For.Name]?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                {
                    TagHelperAttribute classAttribute;

                    if (output.Attributes.TryGetAttribute("class", out classAttribute))
                    {
                        output.Attributes.SetAttribute("class", classAttribute.Value + " " + HasValidationErrorClassName);
                    }
                    else
                    {
                        output.Attributes.Add("class", HasValidationErrorClassName);
                    }
                }

                // We check for whitespace to detect scenarios such as:
                // <span validation-for="Name">
                // </span>
                if (!output.IsContentModified)
                {
                    var childContent = await output.GetChildContentAsync();
                    output.Content.SetHtmlContent(childContent);
                }
            }
        }
    }
}