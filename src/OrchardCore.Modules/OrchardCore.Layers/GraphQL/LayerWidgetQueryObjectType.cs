using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.GraphQL;

public class LayerWidgetQueryObjectType : ObjectGraphType<ContentItem>
{
    public LayerWidgetQueryObjectType()
    {
        Name = "LayerWidget";

        Field<StringGraphType>("zone")
            .Description("The name of the widget's zone.")
            .Resolve(context => context.Source.As<LayerMetadata>()?.Zone);

        Field<DecimalGraphType>("position")
            .Description("The position of the widget in the zone.")
            .Resolve(context => context.Source.As<LayerMetadata>()?.Position);

        Field<BooleanGraphType>("renderTitle")
            .Description("Whether to render the widget's title.")
            .Resolve(context => context.Source.As<LayerMetadata>()?.RenderTitle);

        Field<ContentItemInterface>("widget")
            .Description("A widget on this layer.")
            .Resolve(context => context.Source);
    }
}
