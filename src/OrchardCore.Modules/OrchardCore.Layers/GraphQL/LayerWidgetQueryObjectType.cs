using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;

namespace OrchardCore.Layers.GraphQL
{
    public class LayerWidgetQueryObjectType : ObjectGraphType<IContentItem>
    {
        public LayerWidgetQueryObjectType()
        {
            Name = "LayerWidget";

            Field<StringGraphType>(
                "zone", 
                "The name of the widget's zone.", 
                resolve: context => AsLayerMetadata(context.Source, x => x.Zone));

            Field<StringGraphType>(
                "position", 
                "The position of the widget in the zone.", 
                resolve: context => AsLayerMetadata(context.Source, x => x.Position));

            Field<StringGraphType>(
                "renderTitle", 
                "Whether to render the widget's title.", 
                resolve: context => AsLayerMetadata(context.Source, x => x.RenderTitle));

            Field<ContentItemInterface>(
                "widget",
                "A widget on this layer.",
                resolve: context => context.Source);
        }

        private string AsLayerMetadata(IContentItem contentItem, Expression<Func<LayerMetadata, string>> propAccessor)
        {
            var layerMetadata = contentItem.As<LayerMetadata>();
            if(layerMetadata == null) return null;
            return propAccessor(layerMetadata);
        }
    }
}