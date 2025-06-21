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
            var indexProfileManager = scope.ServiceProvider.GetRequiredService<IIndexProfileManager>();

            var indexes = model.IncludeAll
            ? await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName)
.ConfigureAwait(false) : (await indexProfileManager.GetByProviderAsync(AzureAISearchConstants.ProviderName).ConfigureAwait(false))
                .Where(x => model.Indices.Contains(x.IndexName, StringComparer.OrdinalIgnoreCase));

            foreach (var index in indexes)
            {
                await indexProfileManager.ResetAsync(index).ConfigureAwait(false);
                await indexProfileManager.UpdateAsync(index).ConfigureAwait(false);
                await indexProfileManager.SynchronizeAsync(index).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }
}
