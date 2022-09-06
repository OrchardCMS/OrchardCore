using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes
{
    /// <summary>
    /// This recipe step resets a Elasticsearch index.
    /// </summary>
    public class ElasticIndexResetStep : IRecipeStepHandler
    {
        private readonly ElasticIndexSettingsService _elasticIndexSettingsService;
        private readonly ElasticIndexingService _elasticIndexingService;
        private readonly ElasticIndexManager _elasticIndexManager;

        public ElasticIndexResetStep(
            ElasticIndexSettingsService elasticIndexSettingsService,
            ElasticIndexingService elasticIndexingService,
            ElasticIndexManager elasticIndexManager)
        {
            _elasticIndexSettingsService = elasticIndexSettingsService;
            _elasticIndexManager = elasticIndexManager;
            _elasticIndexingService = elasticIndexingService;
        }

        private async Task ResetIndexAsync(string indexName)
        {
            var elasticIndexSettings = await _elasticIndexSettingsService.GetSettingsAsync(indexName);

            if (elasticIndexSettings != null)
            {
                if (!await _elasticIndexManager.Exists(indexName))
                {
                    await _elasticIndexingService.CreateIndexAsync(elasticIndexSettings);
                    await _elasticIndexingService.ProcessContentItemsAsync(indexName);
                }
                else
                {
                    await _elasticIndexingService.ResetIndex(indexName);
                    await _elasticIndexingService.ProcessContentItemsAsync(indexName);
                }
            }
            await Task.CompletedTask;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "elastic-index-reset", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];
            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-index-reset", async (shellScope) => {
                    foreach (var indexName in indices)
                    {
                        await ResetIndexAsync(indexName.ToObject<string>());
                    }
                });
            }
        }
    }
}
