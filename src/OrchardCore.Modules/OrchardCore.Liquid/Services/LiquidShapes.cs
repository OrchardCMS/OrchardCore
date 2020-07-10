using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
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

            model.Html = await liquidTemplateManager.RenderAsync(model.LiquidPart.Liquid, _htmlEncoder, shapeDisplayContext.DisplayContext.Value,
                scope => scope.SetValue("ContentItem", model.ContentItem));
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("LiquidPart").OnProcessing(BuildViewModelAsync);
            builder.Describe("LiquidPart_Summary").OnProcessing(BuildViewModelAsync);
        }
    }
}
