using System;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes
{
    /// <summary>
    /// This recipe step is used to sync Elasticsearch and Lucene settings.
    /// </summary>
    public class ElasticSettingsStep : IRecipeStepHandler
    {
        private readonly ElasticIndexingService _elasticIndexingService;

        public ElasticSettingsStep(ElasticIndexingService elasticIndexingService)
        {
            _elasticIndexingService = elasticIndexingService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Settings", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step["ElasticSettings"];

            if (step != null)
            {
                if (step["SyncWithLucene"] != null && (bool)step["SyncWithLucene"])
                {
                    await _elasticIndexingService.SyncSettings();
                }
            }
        }
    }
}
