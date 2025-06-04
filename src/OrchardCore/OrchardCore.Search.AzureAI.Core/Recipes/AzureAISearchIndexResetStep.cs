using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.AzureAI.Deployment;

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
            var indexEntityManager = scope.ServiceProvider.GetRequiredService<IIndexEntityManager>();

            var indexes = model.IncludeAll
            ? await indexEntityManager.GetByProviderAsync(AzureAISearchConstants.ProviderName)
            : (await indexEntityManager.GetByProviderAsync(AzureAISearchConstants.ProviderName))
                .Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexEntityManager.ResetAsync(index);
                await indexEntityManager.UpdateAsync(index);
                await indexEntityManager.SynchronizeAsync(index);
            }
        });
    }
}
