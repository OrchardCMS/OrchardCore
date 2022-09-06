using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes
{
    /// <summary>
    /// This recipe step rebuilds a lucene index.
    /// </summary>
    public class ElasticIndexRebuildStep : IRecipeStepHandler
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
        private readonly ElasticIndexingService _elasticIndexingService;

        public ElasticIndexRebuildStep(
            ElasticIndexSettingsService elasticIndexSettingsService,
            ElasticIndexingService elasticIndexingService
            )
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
            _elasticIndexingService = elasticIndexingService;
        }

        private async Task RebuildIndexAsync(string indexName)
        {
            var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync(indexName);

            if (elasticIndexSettings != null)
            {
                await _elasticIndexingService.RebuildIndexAsync(elasticIndexSettings);
                await _elasticIndexingService.ProcessContentItemsAsync(indexName);
            }
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "elastic-index-rebuild", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];

            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-index-rebuild", async (shellScope) => {
                    foreach (var indexName in indices)
                    {
                        await RebuildIndexAsync(indexName.ToObject<string>());
                    }
                });
            }
        }
    }
}
