using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
        private readonly HtmlEncoder _htmlEncoder;
        private HtmlString _bodyAspect;

        public LiquidPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
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

                    var result = await _liquidTemplateManager.RenderAsync(part.Liquid, _htmlEncoder, model,
                        scope => scope.SetValue("ContentItem", model.ContentItem));

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
