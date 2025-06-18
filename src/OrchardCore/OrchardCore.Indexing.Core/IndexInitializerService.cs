using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class IndexInitializerService : ModularTenantEvents
{
    private readonly IIndexProfileStore _store;
    private readonly IServiceProvider _serviceProvider;
    private readonly ShellSettings _shellSettings;

    private bool _initialized;

    public IndexInitializerService(
        IIndexProfileStore store,
        IServiceProvider serviceProvider,
        ShellSettings shellSettings)
    {
        _store = store;
        _serviceProvider = serviceProvider;
        _shellSettings = shellSettings;
    }

    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        if (!_shellSettings.IsRunning() || _initialized)
        {
            return;
        }

        // If the shell is activated there is no migration in progress.
        if (!ShellScope.Context.IsActivated)
        {
            return;
        }

        _initialized = true;

        var indexes = await _store.GetAllAsync();

        var indexManagers = new Dictionary<string, IIndexManager>();

        foreach (var index in indexes)
        {
            if (!indexManagers.TryGetValue(index.ProviderName, out var indexManager))
            {
                indexManager = _serviceProvider.GetKeyedService<IIndexManager>(index.ProviderName);

                if (indexManager is null)
                {
                    continue;
                }

                indexManagers[index.ProviderName] = indexManager;
            }

            if (await indexManager.ExistsAsync(index.IndexFullName))
            {
                await indexManager.DeleteAsync(index);
            }
        }
    }
}
