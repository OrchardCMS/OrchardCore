using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class RebuildIndexEntityStep : NamedRecipeStepHandler
{
    public const string Key = "RebuildIndexing";

    public RebuildIndexEntityStep()
        : base(Key)
    {
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<IndexEntityDeploymentStep>();

        if (model == null)
        {
            return;
        }

        if (!model.IncludeAll && (model.IndexeIds == null || model.IndexeIds.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("rebuild-indexing", async scope =>
        {
            var indexEntityManager = scope.ServiceProvider.GetService<IIndexEntityManager>();

            var indexes = model.IncludeAll
                ? await indexEntityManager.GetAllAsync()
                : (await indexEntityManager.GetAllAsync()).Where(x => model.IndexeIds.Contains(x.Id, StringComparer.OrdinalIgnoreCase));

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

                await indexEntityManager.ResetAsync(index);
                await indexEntityManager.UpdateAsync(index);
                await indexManager.RebuildAsync(index);
                await indexEntityManager.SynchronizeAsync(index);
            }
        });
    }
}
