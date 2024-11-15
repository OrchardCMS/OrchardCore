using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Deployment;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step rebuilds an Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexRebuildStep : NamedRecipeStepHandler
{
    public ElasticsearchIndexRebuildStep()
        : base("elastic-index-rebuild")
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<ElasticsearchIndexRebuildDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("elastic-index-rebuild", async scope =>
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
                        await elasticIndexingService.RebuildIndexAsync(elasticIndexSettings);
                    }
                }

                await elasticIndexingService.ProcessContentItemsAsync(indexNames);
            });
        }
    }
}
