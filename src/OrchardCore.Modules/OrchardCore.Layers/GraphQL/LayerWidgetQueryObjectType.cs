using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.GraphQL
{
    public class LayerWidgetQueryObjectType : ObjectGraphType<ContentItem>
    {
        public LayerWidgetQueryObjectType()
        {
            Name = "LayerWidget";

            Field<StringGraphType>(
                "zone",
                "The name of the widget's zone.",
                resolve: context => context.Source.As<LayerMetadata>()?.Zone);

            Field<DecimalGraphType>(
                "position",
                "The position of the widget in the zone.",
                resolve: context => context.Source.As<LayerMetadata>()?.Position);

            Field<BooleanGraphType>(
                "renderTitle",
                "Whether to render the widget's title.",
                resolve: context => context.Source.As<LayerMetadata>()?.RenderTitle);

            Field<ContentItemInterface>(
                "widget",
                "A widget on this layer.",
                resolve: context => context.Source);
        }
    }
}
