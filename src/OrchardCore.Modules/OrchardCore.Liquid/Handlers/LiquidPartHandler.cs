using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid.Models;
using OrchardCore.Liquid.ViewModels;

namespace OrchardCore.Liquid.Handlers
{
    public class LiquidPartHandler : ContentPartHandler<LiquidPart>
    {
        private readonly ILiquidTemplateManager _liquidTemplateManager;

        private HtmlString _bodyAspect;

        public LiquidPartHandler(ILiquidTemplateManager liquidTemplateManager)
        {
            _liquidTemplateManager = liquidTemplateManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LiquidPart part)
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
                    var model = new LiquidPartViewModel()
                    {
                        LiquidPart = part,
                        ContentItem = part.ContentItem
                    };

                    var templateContext = new TemplateContext();
                    templateContext.SetValue("ContentItem", part.ContentItem);
                    templateContext.MemberAccessStrategy.Register<LiquidPartViewModel>();
                    templateContext.SetValue("Model", model);

                    var result = await _liquidTemplateManager.RenderAsync(part.Liquid, HtmlEncoder.Default, templateContext);
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
