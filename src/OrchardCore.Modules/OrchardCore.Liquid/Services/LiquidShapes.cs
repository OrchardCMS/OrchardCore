using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.Liquid.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Services;

[RequireFeatures("OrchardCore.Contents")]
public class LiquidShapes(HtmlEncoder htmlEncoder) : ShapeTableProvider
{
    private readonly HtmlEncoder _htmlEncoder = htmlEncoder;

    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("LiquidPart").OnProcessing(BuildViewModelAsync);
        builder.Describe("LiquidPart_Summary").OnProcessing(BuildViewModelAsync);
        builder.Describe("LiquidField").OnProcessing(BuildViewModelAsync);

        return ValueTask.CompletedTask;
    }

    private async Task BuildViewModelAsync(ShapeDisplayContext shapeDisplayContext)
    {
        if (shapeDisplayContext.Shape is LiquidPartViewModel partModel && partModel.LiquidPart is not null)
        {
            partModel.Html = await RenderAsync(
                shapeDisplayContext,
                partModel.LiquidPart.Liquid,
                partModel.ContentItem);

            return;
        }

        if (shapeDisplayContext.Shape is LiquidFieldViewModel fieldModel && fieldModel.Field is not null)
        {
            fieldModel.Html = await RenderAsync(
                shapeDisplayContext,
                fieldModel.Field.Liquid,
                fieldModel.ContentItem);
        }
    }

    private async Task<string> RenderAsync(
        ShapeDisplayContext shapeDisplayContext,
        string liquid,
        ContentItem contentItem)
    {
        var liquidTemplateManager = shapeDisplayContext.ServiceProvider.GetRequiredService<ILiquidTemplateManager>();
        return await liquidTemplateManager.RenderStringAsync(liquid, _htmlEncoder, shapeDisplayContext.DisplayContext.Value,
            new Dictionary<string, FluidValue>()
            {
                ["ContentItem"] = new ObjectValue(contentItem),
            });
    }
}
