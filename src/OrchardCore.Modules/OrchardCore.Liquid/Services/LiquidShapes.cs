using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Liquid.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Services
{
    [RequireFeatures("OrchardCore.Contents")]
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

            model.Html = await liquidTemplateManager.RenderStringAsync(model.LiquidPart.Liquid, _htmlEncoder, shapeDisplayContext.DisplayContext.Value,
                new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("LiquidPart").OnProcessing(BuildViewModelAsync);
            builder.Describe("LiquidPart_Summary").OnProcessing(BuildViewModelAsync);
        }
    }
}
