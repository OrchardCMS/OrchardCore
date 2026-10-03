using GraphQL.Types;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.GraphQL;

public class LayerWidgetQueryObjectType : ObjectGraphType<ContentItem>
{
    public LayerWidgetQueryObjectType(IStringLocalizer<LayerWidgetQueryObjectType> S)
    {
        Name = "LayerWidget";

        Field<StringGraphType>("zone")
            .Description(S["The name of the widget's zone."])
            .Resolve(context => context.Source.TryGet<LayerMetadata>(out var metadata) ? metadata.Zone : null);

        Field<DecimalGraphType>("position")
            .Description(S["The position of the widget in the zone."])
            .Resolve(context => context.Source.TryGet<LayerMetadata>(out var metadata) ? metadata.Position : null);

        Field<BooleanGraphType>("renderTitle")
            .Description(S["Whether to render the widget's title."])
            .Resolve(context => context.Source.TryGet<LayerMetadata>(out var metadata) ? metadata.RenderTitle : null);

        Field<ContentItemInterface>("widget")
            .Description(S["A widget on this layer."])
            .Resolve(context => context.Source);
    }
}
