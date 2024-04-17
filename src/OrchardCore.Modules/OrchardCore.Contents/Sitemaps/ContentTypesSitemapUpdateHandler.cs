using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Locking.Distributed;
using OrchardCore.Sitemaps.Handlers;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapUpdateHandler : ContentHandlerBase
    {
        public override Task PublishedAsync(PublishContentContext context) => UpdateSitemapDeferredAsync(context);

        public override Task UnpublishedAsync(PublishContentContext context) => UpdateSitemapDeferredAsync(context);

        // Doing the update in a deferred and synchronized way makes sure that two simultaneous content item updates
        // don't cause a ConcurrencyException due to the same sitemap document being updated.
        private static Task UpdateSitemapDeferredAsync(ContentContextBase context)
        {
            var contentItemId = context.ContentItem.ContentItemId;

            ShellScope.AddDeferredTask(async scope =>
            {
                var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();

                var timeout = TimeSpan.FromMilliseconds(20_000);
                (var locker, var locked) = await distributedLock.TryAcquireLockAsync("SITEMAPS_UPDATE_LOCK", timeout, timeout);

                if (!locked)
                {
                    throw new TimeoutException($"Couldn't acquire a lock to update the sitemap within {timeout.Seconds} seconds.");
                }
                else
                {
                    using (locker)
                    {
                        var contentManager = scope.ServiceProvider.GetRequiredService<IContentManager>();
                        var sitemapUpdateHandler = scope.ServiceProvider.GetRequiredService<ISitemapUpdateHandler>();

                        var updateContext = new SitemapUpdateContext
                        {
                            UpdateObject = await contentManager.GetAsync(contentItemId),
                        };

                        await sitemapUpdateHandler.UpdateSitemapAsync(updateContext);
                    }
                }
            });

            return Task.CompletedTask;
        }
    }
}
