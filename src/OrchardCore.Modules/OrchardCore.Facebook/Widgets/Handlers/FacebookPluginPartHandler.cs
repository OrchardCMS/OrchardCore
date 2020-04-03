using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Liquid;

namespace OrchardCore.Facebook.Widgets.Handlers
{
    public class FacebookPluginPartHandler : ContentPartHandler<FacebookPluginPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        public FacebookPluginPartHandler(ILiquidTemplateManager liquidTemplateManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, FacebookPluginPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                try
                {
                    var result = await _liquidTemplateManager.RenderAsync(part.Liquid, System.Text.Encodings.Web.HtmlEncoder.Default, new TemplateContext());
                    bodyAspect.Body = new HtmlString(result);
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}
