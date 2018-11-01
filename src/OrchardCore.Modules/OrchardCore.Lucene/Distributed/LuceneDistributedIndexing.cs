using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DeferredTasks;
using OrchardCore.Distributed;
using OrchardCore.Environment.Shell;
using OrchardCore.Indexing;
using OrchardCore.Modules;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Lucene.Distributed
{
    public class LuceneDistributedIndexing : ContentHandlerBase, IModularTenantEvents
    {
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IMessageBus _messageBus;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LuceneIndexManager _luceneIndexManager;
        private readonly ILogger<LuceneDistributedIndexing> _logger;

        public LuceneDistributedIndexing(
            IShellHost shellHost,
            ShellSettings shellSettings,
            IEnumerable<IMessageBus> _messageBuses,
            IHttpContextAccessor httpContextAccessor,
            LuceneIndexManager luceneIndexManager,
            ILogger<LuceneDistributedIndexing> logger)
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

                // Validate the message {event}:{contentitemid}
                if (tokens.Length != 2 || tokens[1].Length == 0)
                {
                    return;
                }

                using (var scope = _shellHost.GetScopeAsync(_shellSettings).GetAwaiter().GetResult())
                {
                    if (tokens[0] == "Published")
                    {
                        var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();

                        var contentItem = contentManager.GetAsync(tokens[1]).GetAwaiter().GetResult();

                        if (contentItem == null)
                        {
                            return;
                        }

                        var buildIndexContext = new BuildIndexContext(new DocumentIndex(tokens[1]), contentItem, new string[] { contentItem.ContentType });
                        var contentItemIndexHandlers = scope.ServiceProvider.GetServices<IContentItemIndexHandler>();
                        contentItemIndexHandlers.InvokeAsync(x => x.BuildIndexAsync(buildIndexContext), _logger).GetAwaiter().GetResult();

                        foreach (var index in _luceneIndexManager.List())
                        {
                            _luceneIndexManager.DeleteDocuments(index, new string[] { tokens[1] });
                            _luceneIndexManager.StoreDocuments(index, new DocumentIndex[] { buildIndexContext.DocumentIndex });
                        }
                    }

                    else if (tokens[0] == "Removed" || tokens[0] == "Unpublished")
                    {
                        foreach (var index in _luceneIndexManager.List())
                        {
                            _luceneIndexManager.DeleteDocuments(index, new string[] { tokens[1] });
                        }
                    }
                }
            }) ?? Task.CompletedTask);
        }

        public Task TerminatingAsync() { return Task.CompletedTask; }
        public Task TerminatedAsync() { return Task.CompletedTask; }

        public override Task PublishedAsync(PublishContentContext context)
        {
            var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices.GetService<IDeferredTaskEngine>();

            deferredTaskEngine?.AddTask(async taskContext =>
            {
                var messageBus = taskContext.ServiceProvider.GetService<IMessageBus>();
                await (_messageBus?.PublishAsync("Indexing", "Published:" + context.ContentItem.ContentItemId) ?? Task.CompletedTask);
            }, order: 50);

            return Task.CompletedTask;
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices.GetService<IDeferredTaskEngine>();

            deferredTaskEngine?.AddTask(async taskContext =>
            {
                var messageBus = taskContext.ServiceProvider.GetService<IMessageBus>();
                await (_messageBus?.PublishAsync("Indexing", "Removed:" + context.ContentItem.ContentItemId) ?? Task.CompletedTask);
            }, order: 50);

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            var deferredTaskEngine = _httpContextAccessor.HttpContext.RequestServices.GetService<IDeferredTaskEngine>();

            deferredTaskEngine?.AddTask(async taskContext =>
            {
                var messageBus = taskContext.ServiceProvider.GetService<IMessageBus>();
                await (_messageBus?.PublishAsync("Indexing", "Unpublished:" + context.ContentItem.ContentItemId) ?? Task.CompletedTask);
            }, order: 50);

            return Task.CompletedTask;
        }
    }
}