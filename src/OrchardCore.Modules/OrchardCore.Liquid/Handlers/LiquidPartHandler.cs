using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
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

        public LiquidPartHandler(ILiquidTemplateManager liquidTemplateManager, HtmlEncoder htmlEncoder)
        {
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LiquidPart part)
        {
            return context.ForAsync<BodyAspect>(async bodyAspect =>
            {
                try
                {
                    var model = new LiquidPartViewModel()
                    {
                        LiquidPart = part,
                        ContentItem = part.ContentItem,
                    };

                    var result = await _liquidTemplateManager.RenderHtmlContentAsync(part.Liquid, _htmlEncoder, model,
                        new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });

                    bodyAspect.Body = result;
                }
                catch
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}
