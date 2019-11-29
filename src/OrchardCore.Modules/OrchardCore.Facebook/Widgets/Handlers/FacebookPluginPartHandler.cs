using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
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
        private HtmlString _bodyAspect;

        public FacebookPluginPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, FacebookPluginPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                if (_bodyAspect != null)
                {
                    bodyAspect.Body = _bodyAspect;
                    return;
                }

                try
                {
                    var model = new FacebookPluginPartViewModel()
                    {
                        Liquid = part.Liquid,
                        FacebookPluginPart = part,
                        ContentItem = part.ContentItem
                    };

                    var templateContext = new TemplateContext();
                    templateContext.SetValue("ContentItem", part.ContentItem);
                    templateContext.MemberAccessStrategy.Register<FacebookPluginPartViewModel>();
                    templateContext.SetValue("Model", model);

                    var result = await _liquidTemplateManager.RenderAsync(part.Liquid, _htmlEncoder, templateContext);
                    bodyAspect.Body = _bodyAspect = new HtmlString(result);
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}
