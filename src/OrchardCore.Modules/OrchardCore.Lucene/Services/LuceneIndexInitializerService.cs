using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Lucene
{
    public class LuceneIndexInitializerService : IModularTenantEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;
        private readonly LuceneIndexSettingsService _luceneIndexSettingsService;
        private readonly LuceneIndexingService _luceneIndexingService;

        public LuceneIndexInitializerService(LuceneIndexSettingsService luceneIndexSettingsService,
            LuceneIndexManager luceneIndexManager,
            LuceneIndexingService luceneIndexingService,
            ShellSettings shellSettings,
            ILogger<LuceneIndexInitializerService> logger)
        {
            _luceneIndexSettingsService = luceneIndexSettingsService;
            _luceneIndexingService = luceneIndexingService;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public async Task ActivatedAsync()
        {
            if (_shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized)
            {
                try
                {
                    var luceneIndexSettings = await _luceneIndexSettingsService.GetSettingsAsync();
                    
                    foreach (var settings in luceneIndexSettings)
                    {
                        await _luceneIndexingService.CreateIndexAsync(settings);
                        await _luceneIndexingService.ProcessContentItemsAsync(settings.IndexName);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while initializing a Lucene index.");
                }
            }
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
