using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class IndexInitializerService : ModularTenantEvents
{
    private readonly IIndexProfileStore _store;
    private readonly IServiceProvider _serviceProvider;

    public IndexInitializerService(
        IIndexProfileStore store,
        IServiceProvider serviceProvider)
    {
        _store = store;
        _serviceProvider = serviceProvider;
    }

    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        var indexes = await _store.GetAllAsync().ConfigureAwait(false);

        var indexManagers = new Dictionary<string, IIndexManager>();

        foreach (var index in indexes)
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

            if (await indexManager.ExistsAsync(index.IndexFullName).ConfigureAwait(false))
            {
                await indexManager.DeleteAsync(index).ConfigureAwait(false);
            }
        }
    }
}
