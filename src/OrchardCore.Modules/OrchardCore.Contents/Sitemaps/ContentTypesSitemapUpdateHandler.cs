using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Locking.Distributed;
using OrchardCore.Sitemaps.Handlers;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapUpdateHandler : ContentHandlerBase
    {
        private readonly IDistributedLock _distributedLock;
        private readonly ISitemapUpdateHandler _sitemapUpdateHandler;

        public ContentTypesSitemapUpdateHandler(IDistributedLock distributedLock, ISitemapUpdateHandler sitemapUpdateHandler)
        {
            _distributedLock = distributedLock;
            _sitemapUpdateHandler = sitemapUpdateHandler;
        }

        public override Task PublishedAsync(PublishContentContext context) => UpdateSitemapDeferredAsync(context);

        public override Task UnpublishedAsync(PublishContentContext context) => UpdateSitemapDeferredAsync(context);

        // Doing the update in a deferred and synchronized way makes sure that two simultaneous content item updates
        // don't cause a ConcurrencyException due to the same sitemap document being updated.
        private async Task UpdateSitemapDeferredAsync(ContentContextBase context)
        {
            var timeout = TimeSpan.FromMilliseconds(20_000);
            (var locker, var locked) = await _distributedLock.TryAcquireLockAsync("SITEMAPS_UPDATE_LOCK", timeout, timeout);

            if (!locked)
            {
                throw new TimeoutException($"Couldn't acquire a lock to update the sitemap within {timeout.Seconds} seconds.");
            }
            else
            {
                using (locker)
                {
                    var updateContext = new SitemapUpdateContext
                    {
                        UpdateObject = context.ContentItem,
                    };

                    await _sitemapUpdateHandler.UpdateSitemapAsync(updateContext);
                }
            }
        }
    }
}
