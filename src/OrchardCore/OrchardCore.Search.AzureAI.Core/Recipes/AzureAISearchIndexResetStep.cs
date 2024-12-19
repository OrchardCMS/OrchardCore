using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;
using OrchardCore.Search.AzureAI.Services;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexResetStep : NamedRecipeStepHandler
{
    public AzureAISearchIndexResetStep()
        : base(AzureAISearchIndexResetDeploymentSource.Name)
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<AzureAISearchIndexResetDeploymentStep>();

        if (model == null)
        {
            return;
        }

        if (!model.IncludeAll && (model.Indices == null || model.Indices.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync(AzureAISearchIndexRebuildDeploymentSource.Name, async scope =>
        {
            var searchIndexingService = scope.ServiceProvider.GetService<AzureAISearchIndexingService>();
            var indexSettingsService = scope.ServiceProvider.GetService<AzureAISearchIndexSettingsService>();
            var indexManager = scope.ServiceProvider.GetRequiredService<AzureAISearchIndexManager>();
            var indexDocumentManager = scope.ServiceProvider.GetRequiredService<AzureAIIndexDocumentManager>();

            var indexSettings = model.IncludeAll
            ? await indexSettingsService.GetSettingsAsync()
            : (await indexSettingsService.GetSettingsAsync()).Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var settings in indexSettings)
            {
                settings.SetLastTaskId(0);
                settings.IndexMappings = await indexDocumentManager.GetMappingsAsync(settings);

                if (!await indexManager.ExistsAsync(settings.IndexName))
                {
                    settings.IndexFullName = indexManager.GetFullIndexName(settings.IndexName);

                    await indexManager.CreateAsync(settings);
                }

                await indexSettingsService.UpdateAsync(settings);
            }

            await searchIndexingService.ProcessContentItemsAsync(indexSettings.Select(settings => settings.IndexName).ToArray());
        });
    }
}
