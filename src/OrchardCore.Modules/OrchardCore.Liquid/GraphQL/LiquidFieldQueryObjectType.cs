using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using Fluid.Values;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Liquid.Fields;
using OrchardCore.Liquid.ViewModels;

namespace OrchardCore.Liquid.GraphQL;

public sealed class LiquidFieldQueryObjectType : ObjectGraphType<LiquidField>
{
    public LiquidFieldQueryObjectType(IStringLocalizer<LiquidFieldQueryObjectType> S)
    {
        Name = nameof(LiquidField);
        Description = S["Content stored as a Liquid template."];

        Field<StringGraphType>("liquid")
            .Description(S["the rendered Liquid content"])
            .ResolveLockedAsync(RenderLiquidAsync);
    }

    private static async ValueTask<object> RenderLiquidAsync(IResolveFieldContext<LiquidField> context)
    {
        var serviceProvider = context.RequestServices;
        var contentDefinitionManager = serviceProvider.GetRequiredService<IContentDefinitionManager>();

        var content = (JsonObject)context.Source.Content;
        var paths = content.GetNormalizedPath().Split('.');
        var partName = paths[0];
        var fieldName = paths[1];
        var contentTypeDefinition = await contentDefinitionManager.GetTypeDefinitionAsync(context.Source.ContentItem.ContentType);
        var contentPartDefinition = contentTypeDefinition.Parts.FirstOrDefault(
            part => string.Equals(part.Name, partName, StringComparison.Ordinal));
        var contentPartFieldDefinition = contentPartDefinition.PartDefinition.Fields.FirstOrDefault(
            field => string.Equals(field.Name, fieldName, StringComparison.Ordinal));

        var model = new LiquidFieldViewModel
        {
            Liquid = context.Source.Liquid,
            Field = context.Source,
            Part = context.Source.ContentItem.Get<ContentPart>(partName),
            PartFieldDefinition = contentPartFieldDefinition,
            ContentItem = context.Source.ContentItem,
        };

        var liquidTemplateManager = serviceProvider.GetRequiredService<ILiquidTemplateManager>();
        var htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();

        return await liquidTemplateManager.RenderStringAsync(
            context.Source.Liquid,
            htmlEncoder,
            model,
            new Dictionary<string, FluidValue>
            {
                [nameof(model.ContentItem)] = new ObjectValue(model.ContentItem),
            });
    }
}
