using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Facebook.Widgets.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Facebook.Widgets.Services
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
            var model = shapeDisplayContext.Shape as FacebookPluginPartViewModel;
            var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

            model.Html = await liquidTemplateManager.RenderAsync(model.FacebookPluginPart.Liquid, _htmlEncoder, shapeDisplayContext.DisplayContext.Value,
                scope => scope.SetValue("ContentItem", model.ContentItem));
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("FacebookPluginPart").OnProcessing(BuildViewModelAsync);
            builder.Describe("FacebookPluginPart_Summary").OnProcessing(BuildViewModelAsync);
        }
    }
}
