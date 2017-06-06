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
        private readonly ILiquidManager _liquidManager;

        public LiquidPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            ILiquidManager liquidManager
            )
        {
            _contentDefinitionManager = contentDefinitionManager;
            _liquidManager = liquidManager;
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, LiquidPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {
                var template = _liquidManager.GetTemplate(part.Liquid);

                if (template == null)
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
                else
                {
                    var html = template.Render();
                    bodyAspect.Body = new HtmlString(html);
                }
            });
        }
    }
}