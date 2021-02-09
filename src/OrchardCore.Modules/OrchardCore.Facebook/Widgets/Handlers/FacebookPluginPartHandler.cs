using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Facebook.Widgets.Handlers
{
    public class FacebookPluginPartHandler : ContentPartHandler<FacebookPluginPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;

        public FacebookPluginPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, FacebookPluginPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                try
                {
                    var model = new FacebookPluginPartViewModel()
                    {
                        Liquid = part.Liquid,
                        FacebookPluginPart = part,
                        ContentItem = part.ContentItem
                    };

                    var result = await _liquidTemplateManager.RenderAsync(part.Liquid, _htmlEncoder, model,
                        context => context.SetValue("ContentItem", model.ContentItem));

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
