using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Search.Lucene;

public class LuceneIndexInitializerService : ModularTenantEvents
{
    private readonly ShellSettings _shellSettings;

    public LuceneIndexInitializerService(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public override Task ActivatedAsync()
    {
        if (_shellSettings.IsRunning())
        {
            ShellScope.AddDeferredTask(async scope =>
            {
                var luceneIndexSettingsService = scope.ServiceProvider.GetRequiredService<LuceneIndexSettingsService>();
                var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();
                var indexManager = scope.ServiceProvider.GetRequiredService<LuceneIndexManager>();

                var luceneIndexSettings = await luceneIndexSettingsService.GetSettingsAsync();

                foreach (var settings in luceneIndexSettings)
                {
                    if (!indexManager.Exists(settings.IndexName))
                    {
                        await luceneIndexingService.CreateIndexAsync(settings);
                        await luceneIndexingService.ProcessContentItemsAsync(settings.IndexName);
                    }
                }
            });
        }

        return Task.CompletedTask;
    }
}
