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
            var indexEntityManager = scope.ServiceProvider.GetRequiredService<IIndexEntityManager>();
            var indexManager = scope.ServiceProvider.GetKeyedService<IIndexManager>(AzureAISearchConstants.ProviderName);

            var indexes = model.IncludeAll
            ? await indexEntityManager.GetByProviderAsync(AzureAISearchConstants.ProviderName)
            : (await indexEntityManager.GetByProviderAsync(AzureAISearchConstants.ProviderName))
                .Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexEntityManager.ResetAsync(index);
                await indexEntityManager.UpdateAsync(index);
                await indexManager.RebuildAsync(index);
                await indexEntityManager.SynchronizeAsync(index);
            }
        });
    }
}
