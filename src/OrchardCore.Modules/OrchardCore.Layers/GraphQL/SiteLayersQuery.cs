using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;

namespace OrchardCore.Layers.GraphQL
{
    public class SiteLayersQuery : ISchemaBuilder
    {
        protected readonly IStringLocalizer S;
        private readonly GraphQLContentOptions _graphQLContentOptions;

        public SiteLayersQuery(
            IStringLocalizer<SiteLayersQuery> localizer,
            IOptions<GraphQLContentOptions> graphQLContentOptions)
        {
            S = localizer;
            _graphQLContentOptions = graphQLContentOptions.Value;
        }

        public Task<string> GetIdentifierAsync() => Task.FromResult(String.Empty);

        public Task BuildAsync(ISchema schema)
        {
            if (_graphQLContentOptions.IsHiddenByDefault("SiteLayers"))
            {
                return Task.CompletedTask;
            }

            var field = new FieldType
            {
                Name = "SiteLayers",
                Description = S["Site layers define the rules and zone placement for widgets."],
                Type = typeof(ListGraphType<LayerQueryObjectType>),
                Resolver = new LockedAsyncFieldResolver<IEnumerable<Layer>>(ResolveAsync)
            };

            schema.Query.AddField(field);

            return Task.CompletedTask;
        }

        private async Task<IEnumerable<Layer>> ResolveAsync(IResolveFieldContext resolveContext)
        {
            var layerService = resolveContext.RequestServices.GetService<ILayerService>();
            var allLayers = await layerService.GetLayersAsync();
            return allLayers.Layers;
        }
    }
}
