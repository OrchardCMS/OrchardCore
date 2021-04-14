using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Lucene
{
    public class LuceneIndexInitializerService : IModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneIndexingService _luceneIndexingService;

        public LuceneIndexInitializerService(
            LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneIndexingService luceneIndexingService,
            ShellSettings shellSettings,
            ILogger<LuceneIndexInitializerService> logger)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _luceneIndexingService = luceneIndexingService;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public Task ActivatedAsync()
        {
            if (_shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized)
            {
                ShellScope.AddDeferredTask(async scope =>
                {
                    var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync();
                    
                    foreach (var settings in luceneIndexSettings)
                    {
                        await _luceneIndexingService.CreateIndexAsync(settings);
                        await _luceneIndexingService.ProcessContentItemsAsync(settings.IndexName);
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
