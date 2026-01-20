using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Facebook.Widgets.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.Facebook.Widgets.Services;

public class LiquidShapes(HtmlEncoder htmlEncoder) : ShapeTableProvider
{
    private readonly HtmlEncoder _htmlEncoder = htmlEncoder;

    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("FacebookPluginPart").OnProcessing(BuildViewModelAsync);
        builder.Describe("FacebookPluginPart_Summary").OnProcessing(BuildViewModelAsync);

        return ValueTask.CompletedTask;
    }

    private async Task BuildViewModelAsync(ShapeDisplayContext shapeDisplayContext)
    {
        var model = shapeDisplayContext.Shape as FacebookPluginPartViewModel;
        var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();

        model.Html = await liquidTemplateManager.RenderStringAsync(model.FacebookPluginPart.Liquid, _htmlEncoder, shapeDisplayContext.DisplayContext.Value,
            new Dictionary<string, FluidValue>() { ["ContentItem"] = new ObjectValue(model.ContentItem) });
    }
}
