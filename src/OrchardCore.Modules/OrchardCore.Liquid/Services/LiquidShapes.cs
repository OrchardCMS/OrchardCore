using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid.ViewModels;

namespace OrchardCore.Liquid.Services
{
    public class LiquidShapes : IShapeTableProvider
    {
        private readonly HtmlEncoder _htmlEncoder;

        public LiquidShapes(HtmlEncoder htmlEncoder)
        {
            _htmlEncoder = htmlEncoder;
        }

        private async Task BuildViewModelAsync(ShapeDisplayContext shapeDisplayContext)
        {
            var model = shapeDisplayContext.Shape as LiquidPartViewModel;
            var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var liquidPart = model.LiquidPart;

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", liquidPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<LiquidPartViewModel>();
            await templateContext.ContextualizeAsync(shapeDisplayContext.DisplayContext);

            model.Html = await liquidTemplateManager.RenderAsync(liquidPart.Liquid, _htmlEncoder, templateContext);
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("LiquidPart").OnProcessing(BuildViewModelAsync);
            builder.Describe("LiquidPart_Summary").OnProcessing(BuildViewModelAsync);
        }
    }
}
