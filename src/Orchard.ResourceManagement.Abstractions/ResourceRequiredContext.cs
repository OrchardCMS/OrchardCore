using System;
using Microsoft.AspNetCore.Html;

namespace Orchard.ResourceManagement
{
    public class ResourceRequiredContext
    {
        private const string NotIE = "!IE";

        public ResourceDefinition Resource { get; set; }
        public RequireSettings Settings { get; set; }

        public IHtmlContent GetHtmlContent(RequireSettings baseSettings, string appPath)
        {
            var combinedSettings = baseSettings == null ? Settings : baseSettings.Combine(Settings);

            var tagBuilder = Resource.GetTagBuilder(combinedSettings, appPath);

            if (String.IsNullOrEmpty(combinedSettings.Condition))
            {
                return tagBuilder;
            }

            var builder = new HtmlContentBuilder();

            if (combinedSettings.Condition == NotIE)
            {
                builder.AppendHtml("<!--[if " + combinedSettings.Condition + "]>-->");
            }
            else
            {
                builder.AppendHtml("<!--[if " + combinedSettings.Condition + "]>");
            }

            builder.AppendHtml(tagBuilder);

            if (!string.IsNullOrEmpty(combinedSettings.Condition))
            {
                if (combinedSettings.Condition == NotIE)
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
