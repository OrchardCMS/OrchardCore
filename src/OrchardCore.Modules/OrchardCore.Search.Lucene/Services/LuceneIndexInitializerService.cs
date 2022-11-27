using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Search.Lucene
{
    public class LuceneIndexInitializerService : IModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;

        public LuceneIndexInitializerService(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public Task ActivatedAsync()
        {
            if (_shellSettings.State == TenantState.Running)
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

        public Task ActivatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
