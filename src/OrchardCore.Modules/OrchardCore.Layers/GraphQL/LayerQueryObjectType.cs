using System.Linq.Expressions;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Rules;

namespace OrchardCore.Layers.GraphQL;

public class LayerQueryObjectType : ObjectGraphType<Layer>
{
    public LayerQueryObjectType()
    {
        Name = "Layer";

        Field(layer => layer.Name).Description("The name of the layer.");
        Field<ListGraphType<StringGraphType>, IEnumerable<Condition>>("layerrule")
            .Description("The rule that activates the layer.")
            .Resolve(ctx => ctx.Source.LayerRule.Conditions);
        Field(layer => layer.Description).Description("The description of the layer.");
        Field<ListGraphType<LayerWidgetQueryObjectType>, IEnumerable<ContentItem>>("widgets")
            .Description("The widgets for this layer.")
            .Argument<PublicationStatusGraphType>("status", "publication status of the widgets")
            .ResolveLockedAsync(GetWidgetsForLayerAsync);

        async ValueTask<IEnumerable<ContentItem>> GetWidgetsForLayerAsync(IResolveFieldContext<Layer> context)
        {
            var layerService = context.RequestServices.GetService<ILayerService>();

            var filter = GetVersionFilter(context.GetArgument<PublicationStatusEnum>("status"));
            var widgets = await layerService.GetLayerWidgetsAsync(filter);

            var layerWidgets = widgets?.Where(item =>
            {
                var metadata = item.As<LayerMetadata>();
                if (metadata == null)
                {
                    return false;
                }

                return metadata.Layer == context.Source.Name;
            });

            return layerWidgets;
        }
    }

    private static Expression<Func<ContentItemIndex, bool>> GetVersionFilter(PublicationStatusEnum status) =>
        status switch
        {
            PublicationStatusEnum.Published => x => x.Published,
            PublicationStatusEnum.Draft => x => x.Latest && !x.Published,
            PublicationStatusEnum.Latest => x => x.Latest,
            PublicationStatusEnum.All => x => true,
            _ => x => x.Published,
        };
}
