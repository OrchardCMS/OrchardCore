using System.Threading.Tasks;
using Fluid;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Facebook.Widgets.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Facebook.Widgets.Services
{
    public class LiquidShapes : IShapeTableProvider
    {
        private static async Task BuildViewModelAsync(ShapeDisplayContext shapeDisplayContext)
        {
            var model = shapeDisplayContext.Shape as FacebookPluginPartViewModel;
            var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
            var part = model.FacebookPluginPart;

            var templateContext = new TemplateContext();
            templateContext.SetValue("ContentItem", part.ContentItem);
            templateContext.MemberAccessStrategy.Register<FacebookPluginPartViewModel>();
            await templateContext.ContextualizeAsync(shapeDisplayContext.DisplayContext);

            model.Html = await liquidTemplateManager.RenderAsync(part.Liquid, templateContext);

            model.Liquid = part.Liquid;
            model.FacebookPluginPart = part;
            model.ContentItem = part.ContentItem;
        }

        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("FacebookPluginPart").OnProcessing(BuildViewModelAsync);
            builder.Describe("FacebookPluginPart_Summary").OnProcessing(BuildViewModelAsync);
        }
    }
}
