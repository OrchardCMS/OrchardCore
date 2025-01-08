using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step resets an Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexResetStep : NamedRecipeStepHandler
{
    public ElasticsearchIndexResetStep()
        : base("elastic-index-reset")
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<ElasticsearchIndexResetDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-index-reset", async scope =>
            {
                var elasticIndexingService = scope.ServiceProvider.GetService<ElasticsearchIndexingService>();
                var elasticIndexSettingsService = scope.ServiceProvider.GetService<ElasticsearchIndexSettingsService>();
                var elasticIndexManager = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexManager>();

                var indexNames = model.IncludeAll
                ? (await elasticIndexSettingsService.GetSettingsAsync()).Select(x => x.IndexName).ToArray()
                : model.Indices;

                foreach (var indexName in indexNames)
                {
                    var elasticIndexSettings = await elasticIndexSettingsService.GetSettingsAsync(indexName);

                    if (elasticIndexSettings == null)
                    {
                        continue;
                    }

                    if (!await elasticIndexManager.ExistsAsync(indexName))
                    {
                        await elasticIndexingService.CreateIndexAsync(elasticIndexSettings);
                    }
                    else
                    {
                        await elasticIndexingService.ResetIndexAsync(elasticIndexSettings.IndexName);
                    }
                }

                await elasticIndexingService.ProcessContentItemsAsync(indexNames);
            });
        }
    }
}
