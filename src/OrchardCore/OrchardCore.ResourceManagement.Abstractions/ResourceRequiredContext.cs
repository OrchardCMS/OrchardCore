using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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

            if (String.IsNullOrEmpty(Settings.Condition))
            {
                return tagBuilder;
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

            builder.AppendHtml(tagBuilder);

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
