using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

/// <summary>
/// This recipe step rebuilds an Elasticsearch index.
/// </summary>
public class AzureAISearchIndexRebuildStep : IRecipeStepHandler
{
    public async Task ExecuteAsync(RecipeExecutionContext context)
    {
        if (!string.Equals(context.Name, AzureAISearchIndexRebuildDeploymentSource.Name, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var model = context.Step.ToObject<AzureAISearchIndexRebuildDeploymentStep>();

        if (model != null && (model.IncludeAll || model.Indices.Length > 0))
        {
            await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(AzureAISearchIndexRebuildDeploymentSource.Name, async scope =>
            {
                var searchIndexingService = scope.ServiceProvider.GetService<AzureAISearchIndexingService>();
                var indexSettingsService = scope.ServiceProvider.GetService<AzureAISearchIndexSettingsService>();
                var indexDocumentManager = scope.ServiceProvider.GetRequiredService<AzureAIIndexDocumentManager>();
                var indexManager = scope.ServiceProvider.GetRequiredService<AzureAISearchIndexManager>();

                var indexSettings = model.IncludeAll
                ? await indexSettingsService.GetSettingsAsync()
                : (await indexSettingsService.GetSettingsAsync()).Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

                foreach (var settings in indexSettings)
                {
                    settings.SetLastTaskId(0);
                    settings.IndexMappings = await indexDocumentManager.GetMappingsAsync(settings.IndexedContentTypes);
                    await indexSettingsService.UpdateAsync(settings);

                    await indexManager.RebuildAsync(settings);
                }

                await searchIndexingService.ProcessContentItemsAsync(indexSettings.Select(settings => settings.IndexName).ToArray());
            });
        }
    }
}
