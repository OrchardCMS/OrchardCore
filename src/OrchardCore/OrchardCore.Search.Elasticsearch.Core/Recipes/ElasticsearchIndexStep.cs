using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step creates a Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexStep : NamedRecipeStepHandler
{
    private readonly ElasticsearchIndexingService _elasticIndexingService;
    private readonly ElasticsearchIndexManager _elasticIndexManager;

    public ElasticsearchIndexStep(
        ElasticsearchIndexingService elasticIndexingService,
        ElasticsearchIndexManager elasticIndexManager
        )
        : base("ElasticIndexSettings")
    {
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexingService = elasticIndexingService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        // Elasticserach uses the term "Indices" for the plural of index, but the common spelling in US English is
        // "Indexes". So both are supported to avoid unnecessary spelling problems.
        var indexes = context.Step["Indexes"] as JsonArray ??
                context.Step["Indices"] as JsonArray ??
                [];

        // Get all properties of each objects inside the indexes array. The property name is treated as the index name.
        var settings = indexes
            .SelectMany(index => index.ToObject<Dictionary<string, ElasticIndexSettings>>())
            .Select(WithIndexName);

        // Create the described index only if it doesn't already exist for the current tenant prefix.
        foreach (var setting in settings)
        {
            if (!await _elasticIndexManager.ExistsAsync(setting.IndexName))
            {
                await _elasticIndexingService.CreateIndexAsync(setting);
            }
        }
    }

    private static ElasticIndexSettings WithIndexName(ElasticIndexSettings settings, string name)
    {
        settings.IndexName = name;

        return settings;
    }
}
