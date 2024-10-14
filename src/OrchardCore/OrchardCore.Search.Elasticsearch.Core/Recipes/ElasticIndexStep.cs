using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step creates a Elasticsearch index.
/// </summary>
public sealed class ElasticIndexStep : NamedRecipeStepHandler
{
    private readonly ElasticIndexingService _elasticIndexingService;
    private readonly ElasticIndexManager _elasticIndexManager;

    public ElasticIndexStep(
        ElasticIndexingService elasticIndexingService,
        ElasticIndexManager elasticIndexManager
        )
        : base("ElasticIndexSettings")
    {
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexingService = elasticIndexingService;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step["Indices"] is not JsonArray jsonArray)
        {
            return;
        }

        foreach (var index in jsonArray)
        {
            var elasticIndexSettings = index.ToObject<Dictionary<string, ElasticIndexSettings>>().FirstOrDefault();

            if (!await _elasticIndexManager.ExistsAsync(elasticIndexSettings.Key))
            {
                elasticIndexSettings.Value.IndexName = elasticIndexSettings.Key;
                await _elasticIndexingService.CreateIndexAsync(elasticIndexSettings.Value);
            }
        }
    }
}
