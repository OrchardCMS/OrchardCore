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
        var settings = context.GetIndexSettings<ElasticIndexSettings>();

        // Create the described index only if it doesn't already exist for the current tenant prefix.
        foreach (var setting in settings)
        {
            if (!await _elasticIndexManager.ExistsAsync(setting.IndexName))
            {
                await _elasticIndexingService.CreateIndexAsync(setting);
            }
        }
    }
}
