using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
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

            var model = context.Step.ToObject<ElasticIndexResetDeploymentStep>();

            if (model != null && (model.IncludeAll || model.Indices.Length > 0))
            {
                await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-index-reset", async scope => {

                    var elasticIndexingService = scope.ServiceProvider.GetService<ElasticIndexingService>();
                    var elasticIndexSettingsService = scope.ServiceProvider.GetService<ElasticIndexSettingsService>();
                    var elasticIndexManager = scope.ServiceProvider.GetRequiredService<ElasticIndexManager>();

                    var indices = model.IncludeAll ? (await elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray() : model.Indices;

                    foreach (var indexName in indices)
                    { 
                        var elasticIndexSettings = await elasticIndexSettingsService.GetSettingsAsync(indexName);

                        if (elasticIndexSettings != null)
                        {
                            if (!await elasticIndexManager.Exists(indexName))
                            {
                                await elasticIndexingService.CreateIndexAsync(elasticIndexSettings);
                            }
                            else
                            {
                                await elasticIndexingService.ResetIndexAsync(elasticIndexSettings.IndexName);
                            }

                            await elasticIndexingService.ProcessContentItemsAsync(indexName);
                        }
                    }
                });
            }
        }
    }
}
