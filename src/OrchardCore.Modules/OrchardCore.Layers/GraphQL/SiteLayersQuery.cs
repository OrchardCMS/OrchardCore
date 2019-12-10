using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;

namespace OrchardCore.Layers.GraphQL
{
    public class SiteLayersQuery : ISchemaBuilder
    {
        private readonly IStringLocalizer S;

        public SiteLayersQuery(IStringLocalizer<SiteLayersQuery> localizer)
        {
            S = localizer;
        }

        public Task<IChangeToken> BuildAsync(ISchema schema)
        {
            var field = new FieldType
            {
                Name = "SiteLayers",
                Description = S["Site layers define the rules and zone placement for widgets."],
                Type = typeof(ListGraphType<LayerQueryObjectType>),
                Resolver = new AsyncFieldResolver<IEnumerable<Layer>>(ResolveAsync)
            };

            schema.Query.AddField(field);

            return Task.FromResult<IChangeToken>(null);
        }

        private async Task<IEnumerable<Layer>> ResolveAsync(ResolveFieldContext resolveContext)
        {
            var context = (GraphQLContext)resolveContext.UserContext;
            var layerService = context.ServiceProvider.GetService<ILayerService>();

            var allLayers = await layerService.GetLayersAsync();
            return allLayers.Layers;
        }
    }
}
