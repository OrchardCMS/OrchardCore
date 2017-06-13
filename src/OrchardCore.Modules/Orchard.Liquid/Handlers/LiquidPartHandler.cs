using Fluid;
using Microsoft.AspNetCore.Html;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Models;
using Orchard.Liquid.Model;

namespace Orchard.Liquid.Handlers
{
    public class LiquidPartHandler : ContentPartHandler<LiquidPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public LiquidPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, LiquidPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {
                if (FluidTemplate.TryParse(part.Liquid, out var template, out var errors))
                {
                    var html = template.RenderAsync().GetAwaiter().GetResult();
                    bodyAspect.Body = new HtmlString(html);
                }
                else
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });
        }
    }
}