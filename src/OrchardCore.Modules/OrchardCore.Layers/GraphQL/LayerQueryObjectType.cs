using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.FileStorage;

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
                .Argument<PublicationStatusGraphType>("status", "publication status of the content item", PublicationStatusEnum.Published)
                .Resolve(ctx => {
                    var context = (GraphQLContext)ctx.UserContext;
                    var layerService = context.ServiceProvider.GetService<ILayerService>();
                    
                    var filter = x => x.Published;
                    if (ctx.HasPopulatedArgument("status"))
                    {
                        filter = GetVersionFilter(ctx.GetArgument<PublicationStatusEnum>("status"));
                    }

                    return layerService.GetLayerWidgetsAsync(filter);
                });
        }

        private Expression<Func<ContentItemIndex, bool>> GetVersionFilter(PublicationStatusEnum status)
        {
            switch (status)
            {
                case PublicationStatusEnum.Published: return x => x.Published;
                case PublicationStatusEnum.Draft: return x => x.Draft;
                case PublicationStatusEnum.Latest: return x => x.Latest;
                case PublicationStatusEnum.All: return x => true;
                default: return x => x.Published;
            }
        }
    }
}