using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;

namespace OrchardCore.Search.AzureAI.Recipes;

public sealed class AzureAISearchIndexRebuildStep : NamedRecipeStepHandler
{
    public AzureAISearchIndexRebuildStep()
        : base(AzureAISearchIndexRebuildDeploymentSource.Name)
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<AzureAISearchIndexRebuildDeploymentStep>();

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
            var indexProfileManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
            var indexManager = scope.ServiceProvider.GetKeyedService<IIndexManager>(AzureAISearchConstants.ProviderName);

            var indexProfiles = model.IncludeAll
            ? await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName)
            : (await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName))
                .Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var indexProfile in indexProfiles)
            {
                await indexProfileManager.ResetAsync(indexProfile);
                await indexProfileManager.UpdateAsync(indexProfile);
                await indexManager.RebuildAsync(indexProfile);
                await indexProfileManager.SynchronizeAsync(indexProfile);
            }
        });
    }
}
