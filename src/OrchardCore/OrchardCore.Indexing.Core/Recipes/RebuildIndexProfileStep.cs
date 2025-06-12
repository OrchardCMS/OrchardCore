using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class RebuildIndexProfileStep : NamedRecipeStepHandler
{
    public const string Key = "RebuildIndexProfile";

    public RebuildIndexProfileStep()
        : base(Key)
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexProfileDeploymentStep>();

        if (model == null)
        {
            return;
        }

        if (!model.IncludeAll && (model.IndexIds == null || model.IndexIds.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("rebuild-index-profile", async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetService<IIndexProfileManager>();

            var indexes = model.IncludeAll
                ? await indexProfileManager.GetAllAsync()
                : (await indexProfileManager.GetAllAsync()).Where(x => model.IndexIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase) || model.IndexIds.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            Dictionary<string, IIndexManager> indexManagers = new();

            foreach (var index in indexes)
            {
                if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
                {
                    indexManager = scope.ServiceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                    indexManagers[index.ProviderName] = indexManager;
                }

                if (indexManager is null)
                {
                    continue;
                }

                await indexProfileManager.ResetAsync(index);
                await indexProfileManager.UpdateAsync(index);
                await indexManager.RebuildAsync(index);
                await indexProfileManager.SynchronizeAsync(index);
            }
        });
    }
}
