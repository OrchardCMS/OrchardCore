using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Implementation;
using Orchard.DisplayManagement.Liquid;
using Orchard.Liquid.ViewModels;

namespace Orchard.Liquid.Services
{
    public class LiquidShapes : IShapeTableProvider
    {
        private static async Task BuildViewModelAsync(ShapeDisplayContext shapeDisplayContext)
        {
            var model = shapeDisplayContext.Shape as LiquidPartViewModel;
            var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var liquidPart = model.LiquidPart;

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", liquidPart.ContentItem);
            templateContext.MemberAccessStrategy.Register<LiquidPartViewModel>();
            templateContext.Contextualize(shapeDisplayContext.DisplayContext);

            using (var writer = new StringWriter())
            {
                await liquidTemplateManager.RenderAsync(liquidPart.Liquid, writer, HtmlEncoder.Default, templateContext);
                model.Html = writer.ToString();
            }

            model.Liquid = liquidPart.Liquid;
            model.LiquidPart = liquidPart;
            model.ContentItem = liquidPart.ContentItem;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("LiquidPart").OnProcessingAsync(BuildViewModelAsync);
            builder.Describe("LiquidPart_Summary").OnProcessingAsync(BuildViewModelAsync);
            builder.Describe("LiquidPart_Edit").OnProcessingAsync(BuildViewModelAsync);            
        }
    }
}
