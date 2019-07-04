using System;
using System.Linq;
using System.Linq.Expressions;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.GraphQL.Queries;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;

namespace OrchardCore.Layers.GraphQL
{
    public class LayerQueryObjectType : ObjectGraphType<Layer>
    {
        public LayerQueryObjectType()
        {
            Name = "Layer";

            Field(layer => layer.Name).Description("The name of the layer.");
            Field(layer => layer.Rule).Description("The rule that activates the layer.");
            Field(layer => layer.Description).Description("The description of the layer.");

            Field<ListGraphType<LayerWidgetQueryObjectType>>()
                .Name("widgets")
                .Description("The widgets for this layer.")
                .Argument<PublicationStatusGraphType, PublicationStatusEnum>("status", "publication status of the widgets")
                .ResolveAsync(async ctx => {
                    var context = (GraphQLContext)ctx.UserContext;
                    var layerService = context.ServiceProvider.GetService<ILayerService>();
                    
                    var filter = GetVersionFilter(ctx.GetArgument<PublicationStatusEnum>("status"));
                    var widgets = await layerService.GetLayerWidgetsAsync(filter);

                    var layerWidgets = widgets?.Where(item =>
                    {
                        var metadata = item.As<LayerMetadata>();
                        if (metadata == null) return false;
                        return metadata.Layer == ctx.Source.Name;
                    });

                    return layerWidgets;
                });
        }

        private Expression<Func<ContentItemIndex, bool>> GetVersionFilter(PublicationStatusEnum status)
        {
            switch (status)
            {
                case PublicationStatusEnum.Published: return x => x.Published;
                case PublicationStatusEnum.Draft: return x => x.Latest && !x.Published;
                case PublicationStatusEnum.Latest: return x => x.Latest;
                case PublicationStatusEnum.All: return x => true;
                default: return x => x.Published;
            }
        }
    }
}