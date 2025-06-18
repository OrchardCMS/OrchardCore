using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing.Models;
using OrchardCore.Modules;

namespace OrchardCore.Indexing.Core;

public sealed class ContentIndexInitializerService : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;

    private bool _initialized;

    public ContentIndexInitializerService(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public override Task ActivatedAsync()
    {
        if (!_shellSettings.IsRunning() || _initialized)
        {
            return Task.CompletedTask;
        }

        // If the shell is activated there is no migration in progress.
        if (!ShellScope.Context.IsActivated)
        {
            return Task.CompletedTask;
        }

        _initialized = true;

        return HttpBackgroundJob.ExecuteAfterEndOfRequestAsync("indexing-initialize", async scope =>
        {
            var indexStore = scope.ServiceProvider.GetRequiredService<IIndexProfileStore>();
            var indexingService = scope.ServiceProvider.GetRequiredService<ContentIndexingService>();

            var indexes = await indexStore.GetAllAsync();

            var createdIndexes = new List<IndexProfile>();

            var indexManagers = new Dictionary<string, IIndexManager>();

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

                if (!await indexManager.ExistsAsync(index.IndexFullName))
                {
                    await indexManager.CreateAsync(index);
                }

                createdIndexes.Add(index);
            }

            if (createdIndexes.Count > 0)
            {
                await indexingService.ProcessRecordsAsync(createdIndexes);
            }
        });
    }
}
