using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class IndexInitializerService : ModularTenantEvents
{
    private readonly IIndexEntityStore _indexStore;
    private readonly IServiceProvider _serviceProvider;

    public IndexInitializerService(
        IIndexEntityStore indexEntityStore,
        IServiceProvider serviceProvider)
    {
        _indexStore = indexEntityStore;
        _serviceProvider = serviceProvider;
    }

    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        var indexes = await _indexStore.GetAllAsync();

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

            if (await indexManager.ExistsAsync(index.IndexFullName))
            {
                await indexManager.DeleteAsync(index);
            }
        }
    }
}
