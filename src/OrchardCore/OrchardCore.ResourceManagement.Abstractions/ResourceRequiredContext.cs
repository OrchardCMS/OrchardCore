using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.ResourceManagement
{
    public class ResourceRequiredContext
    {
        private const string NotIE = "!IE";

        public ResourceDefinition Resource { get; set; }
        public RequireSettings Settings { get; set; }
        public IFileVersionProvider FileVersionProvider { get; set; }

        public IHtmlContent GetHtmlContent(string appPath)
        {
            var tagBuilder = Resource.GetTagBuilder(Settings, appPath, FileVersionProvider);

            var attributes = new TagHelperAttributeList();
            foreach (var attribute in tagBuilder.Attributes)
            {
                if (attribute.Key != "integrity")
                {
                    attributes.Add(attribute.Key, attribute.Value);
                    continue;
                }

                attributes.Add(new TagHelperAttribute(attribute.Key, new HtmlString(attribute.Value)));
            }

            var tagHelperOutput = new TagHelperOutput(tagBuilder.TagName, attributes,
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(null))
            {
                TagMode = tagBuilder.TagRenderMode == TagRenderMode.SelfClosing ? TagMode.SelfClosing : TagMode.StartTagAndEndTag
            };

            if (String.IsNullOrEmpty(Settings.Condition))
            {
                return tagHelperOutput;
            }

            var builder = new HtmlContentBuilder();

            if (Settings.Condition == NotIE)
            {
                builder.AppendHtml("<!--[if " + Settings.Condition + "]>-->");
            }
            else
            {
                builder.AppendHtml("<!--[if " + Settings.Condition + "]>");
            }

            builder.AppendHtml(tagHelperOutput);

            if (!string.IsNullOrEmpty(Settings.Condition))
            {
                if (Settings.Condition == NotIE)
                {
                    builder.AppendHtml("<!--<![endif]-->");
                }
                else
                {
                    builder.AppendHtml("<![endif]-->");
                }
            }

            return builder;
        }
    }
}
