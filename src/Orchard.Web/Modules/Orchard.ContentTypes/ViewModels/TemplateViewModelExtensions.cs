using Microsoft.AspNet.Mvc.Rendering;
using Orchard.ContentManagement.ViewModels;
using Orchard.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.ContentTypes.ViewModels
{
    public static class TemplateViewModelExtensions
    {
        public static async Task RenderTemplatesAsync(this IHtmlHelper html, IEnumerable<TemplateViewModel> templates)
        {
            if (templates == null)
                return;

            foreach (var template in templates.OrderByDescending(t => t.Position, new FlatPositionComparer()))
            {
                await html.RenderTemplatesAsync(template);
            }
        }

        public static async Task RenderTemplatesAsync(this IHtmlHelper html, TemplateViewModel template)
        {
            if (template.WasUsed)
                return;

            template.WasUsed = true;

            var templateInfo = html.ViewContext.ViewData.TemplateInfo;
            var htmlFieldPrefix = templateInfo.HtmlFieldPrefix;
            try
            {
                templateInfo.HtmlFieldPrefix = templateInfo.GetFullHtmlFieldName(template.Prefix);
                await html.RenderPartialAsync(template.TemplateName, template.Model, html.ViewContext.ViewData);
            }
            finally
            {
                templateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            }
        }
    }
}
