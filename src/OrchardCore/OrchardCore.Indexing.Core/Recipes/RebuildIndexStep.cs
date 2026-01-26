using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Indexing.Core.Deployments;
using OrchardCore.Locking.Distributed;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Indexing.Core.Recipes;

public sealed class RebuildIndexStep : NamedRecipeStepHandler
{
    public const string Key = "RebuildIndex";

    public RebuildIndexStep()
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

        if (!model.IncludeAll && (model.IndexNames == null || model.IndexNames.Length == 0))
        {
            return;
        }

        await HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("rebuild-indexes", async scope =>
        {
            var indexProfileManager = scope.ServiceProvider.GetService<IIndexProfileManager>();
            var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();

            var indexes = model.IncludeAll
                ? await indexProfileManager.GetAllAsync()
                : (await indexProfileManager.GetAllAsync()).Where(x => model.IndexNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase));

            Dictionary<string, IIndexManager> indexManagers = new();

            foreach (var index in indexes)
            {
                // Acquire a distributed lock to prevent concurrent rebuild operations for this index.
                // This ensures that the ResetAsync, UpdateAsync, RebuildAsync, and SynchronizeAsync
                // sequence is not interrupted by other rebuild operations.
                (var locker, var isLocked) = await distributedLock.TryAcquireLockAsync(
                    $"RebuildIndexStep-{index.Id}",
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromMinutes(15));

                if (!isLocked)
                {
                    // Skip this index if we can't acquire the lock (another rebuild is in progress).
                    continue;
                }

                try
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
                    if (await indexManager.RebuildAsync(index))
                    {
                        await indexProfileManager.SynchronizeAsync(index);
                    }
                }
                finally
                {
                    await locker.DisposeAsync();
                }
            }
        });
    }
}
