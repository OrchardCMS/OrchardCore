using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DeferredTasks;
using OrchardCore.Distributed;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.Lucene.Distributed
{
    public class DistributedIndexing : ContentHandlerBase, IModularTenantEvents
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IMessageBus _messageBus;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly ILogger<DistributedIndexing> _logger;

        public DistributedIndexing(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IEnumerable<IMessageBus> _messageBuses,
            IHttpContextAccessor httpContextAccessor,
            LuceneIndexManager luceneIndexManager,
            ILogger<DistributedIndexing> logger)
        {
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _httpContextAccessor = httpContextAccessor;
            _luceneIndexManager = luceneIndexManager;
            _messageBus = _messageBuses.LastOrDefault();
            _logger = logger;
        }

        public Task ActivatingAsync() { return Task.CompletedTask; }

        public Task ActivatedAsync()
        {
            return (_messageBus?.SubscribeAsync("Indexing", (channel, message) =>
            {
                var tokens = message.Split(':').ToArray();

                // {eventname}:{indexname or contentitemid}
                if (tokens.Length != 2 || tokens[1].Length == 0)
                {
                    return;
                }

                if (tokens[0] == "Reset")
                {
                    using (var scope = _shellHost.GetScopeAsync(_shellSettings).GetAwaiter().GetResult())
                    {
                        var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();

                        luceneIndexingService.ResetIndex(tokens[1]);
                        luceneIndexingService.ProcessContentItemsAsync().GetAwaiter().GetResult();
                    }
                }

                else if (tokens[0] == "Create" || tokens[0] == "Rebuild")
                {
                    using (var scope = _shellHost.GetScopeAsync(_shellSettings).GetAwaiter().GetResult())
                    {
                        var luceneIndexingService = scope.ServiceProvider.GetRequiredService<LuceneIndexingService>();
                        luceneIndexingService.RebuildIndex(tokens[1]);
                        luceneIndexingService.ProcessContentItemsAsync().GetAwaiter().GetResult();
                    }
                }

                else if (tokens[0] == "Delete")
                {
                    _luceneIndexManager.DeleteIndex(tokens[1]);
                }

                else if (tokens[0] == "Published")
                {
                    using (var scope = _shellHost.GetScopeAsync(_shellSettings).GetAwaiter().GetResult())
                    {
                        var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                        var contentItem = contentManager.GetAsync(tokens[1]).GetAwaiter().GetResult();

                        if (contentItem == null)
                        {
                            return;
                        }

                        var context = new BuildIndexContext(new DocumentIndex(tokens[1]), contentItem, new string[] { contentItem.ContentType });
                        var indexHandlers = scope.ServiceProvider.GetServices<IContentItemIndexHandler>();
                        indexHandlers.InvokeAsync(x => x.BuildIndexAsync(context), _logger).GetAwaiter().GetResult();

                        foreach (var index in _luceneIndexManager.List())
                        {
                            _luceneIndexManager.DeleteDocuments(index, new string[] { tokens[1] });
                            _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { context.DocumentIndex });
                        }
                    }
                }

                else if (tokens[0] == "Removed" || tokens[0] == "Unpublished")
                {
                    foreach (var index in _luceneIndexManager.List())
                    {
                        _luceneIndexManager.DeleteDocuments(index, new string[] { tokens[1] });
                    }
                }
            }) ?? Task.CompletedTask);
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }
        public Task TerminatedAsync() { return Task.CompletedTask; }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return PublishAsync(context.ContentItem, "Published");
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return PublishAsync(context.ContentItem, "Removed");
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            return PublishAsync(context.ContentItem, "Unpublished");
        }

        private Task PublishAsync(ContentItem contentItem, string eventName)
        {
            var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices.GetService<IDeferredTaskEngine>();

            deferredTaskEngine?.AddTask(async taskContext =>
            {
                var messageBus = taskContext.ServiceProvider.GetService<IMessageBus>();
                await (_messageBus?.PublishAsync("Indexing", eventName + ':' + contentItem.ContentItemId) ?? Task.CompletedTask);
            }, order: 50);

            return Task.CompletedTask;
        }
    }
}