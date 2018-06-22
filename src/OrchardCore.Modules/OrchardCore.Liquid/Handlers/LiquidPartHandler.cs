using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Models;
using OrchardCore.Liquid.Model;

namespace OrchardCore.Liquid.Handlers
{
    public class LiquidPartHandler : ContentPartHandler<LiquidPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public LiquidPartHandler(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, LiquidPart part)
        {
            context.For<BodyAspect>(bodyAspect =>
            {
                if (FluidTemplate.TryParse(part.Liquid, out var template, out var errors))
                {
                    //var html = template.RenderAsync().GetAwaiter().GetResult();
                    //bodyAspect.Body = new HtmlString(html);
                }
                else
                {
                    bodyAspect.Body = HtmlString.Empty;
                }
            });

            return Task.CompletedTask;
        }
    }
}