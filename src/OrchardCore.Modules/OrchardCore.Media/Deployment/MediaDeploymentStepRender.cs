using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Deployment
{
    public static class MediaDeploymentStepRender
    {
        public static IHtmlContent RenderMediaStoreEntries(this IHtmlHelper<MediaDeploymentStepViewModel> helper, MediaDeploymentStepViewModel model)
        {
            var contentBuilder = new HtmlContentBuilder();

            contentBuilder.AppendHtmlLine("<ul class=\"list-group\">");

            foreach (var entry in model.Entries)
            {
                var checkd = model.Paths?.Contains(entry.Path);
                var hasChildren = entry.Entries?.Length != 0;

                contentBuilder
                    .AppendHtmlLine("<li class=\"list-group-item\">")
                    .AppendHtmlLine("    <div class=\"form-check\">")
                    .AppendHtmlLine("        <label class=\"form-check-label\">");

                if (hasChildren)
                {
                    contentBuilder.AppendHtmlLine($"            <input class=\"form-check-input\" data-entry-type=\"dir\" type=\"checkbox\" checked=\"{checkd}\">");
                }
                else
                {
                    contentBuilder.AppendHtmlLine($"            <input class=\"form-check-input\" data-entry-type=\"file\" type=\"checkbox\" name=\"{helper.NameFor(m => m.Paths)}\" value=\"{entry.Path}\" checked=\"{checkd}\">");
                }

                contentBuilder
                    .AppendHtmlLine($"            {entry.Name}")
                    .AppendHtmlLine("        </label>")
                    .AppendHtmlLine("    </div>")
                    .AppendHtmlLine("</li>");

                if (hasChildren)
                {
                    contentBuilder
                        .AppendHtmlLine("<li class=\"list-group-item\">")
                        .AppendHtml(RenderMediaStoreEntries(helper, new MediaDeploymentStepViewModel
                        {
                            IncludeAll = model.IncludeAll,
                            Paths = model.Paths,
                            Entries = entry.Entries
                        }))
                        .AppendHtmlLine("</li>");
                }
            }

            contentBuilder.AppendHtmlLine("</ul>");

            return contentBuilder;
        }
    }
}