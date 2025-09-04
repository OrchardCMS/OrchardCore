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

namespace OrchardCore.Layers.GraphQL;

public sealed class SiteLayersQuery : ISchemaBuilder
{
    private readonly GraphQLContentOptions _graphQLContentOptions;

    internal readonly IStringLocalizer S;

    public SiteLayersQuery(
        IOptions<GraphQLContentOptions> graphQLContentOptions,
        IStringLocalizer<SiteLayersQuery> localizer)
    {
        _graphQLContentOptions = graphQLContentOptions.Value;
        S = localizer;
    }

    public Task<string> GetIdentifierAsync()
        => Task.FromResult(string.Empty);

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
            Resolver = new LockedAsyncFieldResolver<IEnumerable<Layer>>(ResolveAsync),
        };

        schema.Query.AddField(field);

        return Task.CompletedTask;
    }

    private async ValueTask<IEnumerable<Layer>> ResolveAsync(IResolveFieldContext resolveContext)
    {
        var layerService = resolveContext.RequestServices.GetService<ILayerService>();

        var allLayers = await layerService.GetLayersAsync();

        return allLayers.Layers;
    }
}
