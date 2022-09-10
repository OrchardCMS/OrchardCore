using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes
{
    /// <summary>
    /// This recipe step resets an Elasticsearch index.
    /// </summary>
    public class ElasticIndexResetStep : IRecipeStepHandler
    {
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "elastic-index-reset", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var indices = context.Step["Indices"];

            if (indices != null)
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-index-reset", async scope => {

                    var elasticIndexingService = scope.ServiceProvider.GetService<ElasticIndexingService>();
                    var elasticIndexSettingsService = scope.ServiceProvider.GetService<ElasticIndexSettingsService>();

                    foreach (var indexToken in indices)
                    {
                        var indexName = indexToken.ToObject<string>();

                        var elasticIndexSettings = await elasticIndexSettingsService.GetSettingsAsync(indexName);

                        if (elasticIndexSettings != null)
                        {
                            await elasticIndexingService.ResetIndexAsync(elasticIndexSettings.IndexName);
                            await elasticIndexingService.ProcessContentItemsAsync(indexName);
                        }
                    }
                });
            }
        }
    }
}
