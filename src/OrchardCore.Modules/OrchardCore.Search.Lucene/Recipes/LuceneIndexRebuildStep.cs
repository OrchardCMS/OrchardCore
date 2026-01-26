using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Indexing;
using OrchardCore.Locking.Distributed;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Search.Lucene.Recipes;

/// <summary>
/// This recipe step rebuilds a Lucene index.
/// </summary>
public sealed class LuceneIndexRebuildStep : NamedRecipeStepHandler
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IServiceProvider _serviceProvider;

    public LuceneIndexRebuildStep(
        IIndexProfileManager indexProfileManager,
        IServiceProvider serviceProvider)
        : base("lucene-index-rebuild")
    {
        _indexProfileManager = indexProfileManager;
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<LuceneIndexRebuildStepModel>();

        if (model.IncludeAll || model.Indices.Length > 0)
        {
            var indexes = model.IncludeAll
            ? (await _indexProfileManager.GetByProviderAsync(LuceneConstants.ProviderName))
            : (await _indexProfileManager.GetByProviderAsync(LuceneConstants.ProviderName)).Where(x => model.Indices.Contains(x.IndexName));

            var indexManagers = new Dictionary<string, IIndexManager>();
            var distributedLock = _serviceProvider.GetRequiredService<IDistributedLock>();

            foreach (var index in indexes)
            {
                // Acquire a distributed lock to prevent concurrent rebuild operations for this index.
                (var locker, var isLocked) = await distributedLock.TryAcquireLockAsync(
                    $"LuceneRebuildStep-{index.Id}",
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
                        indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);
                        indexManagers[index.ProviderName] = indexManager;
                    }

                    if (indexManager is null)
                    {
                        continue;
                    }

                    await _indexProfileManager.ResetAsync(index);
                    await _indexProfileManager.UpdateAsync(index);

                    if (!await indexManager.ExistsAsync(index.IndexFullName))
                    {
                        await indexManager.CreateAsync(index);
                    }
                    else
                    {
                        await indexManager.RebuildAsync(index);
                    }

                    await _indexProfileManager.SynchronizeAsync(index);
                }
                finally
                {
                    await locker.DisposeAsync();
                }
            }
        }
    }

    private sealed class LuceneIndexRebuildStepModel
    {
        public bool IncludeAll { get; set; }

        public string[] Indices { get; set; } = [];
    }
}
