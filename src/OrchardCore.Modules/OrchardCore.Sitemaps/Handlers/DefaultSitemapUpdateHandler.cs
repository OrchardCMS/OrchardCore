using OrchardCore.Locking.Distributed;

namespace OrchardCore.Sitemaps.Handlers;

public class DefaultSitemapUpdateHandler : ISitemapUpdateHandler
{
    private readonly IEnumerable<ISitemapTypeUpdateHandler> _sitemapTypeUpdateHandlers;
    private readonly IDistributedLock _distributedLock;

    public DefaultSitemapUpdateHandler(
        IEnumerable<ISitemapTypeUpdateHandler> sitemapTypeUpdateHandlers,
        IDistributedLock distributedLock)
    {
        _sitemapTypeUpdateHandlers = sitemapTypeUpdateHandlers;
        _distributedLock = distributedLock;
    }

    public async Task UpdateSitemapAsync(SitemapUpdateContext context)
    {
        // Doing the update in a synchronized way makes sure that two simultaneous content item updates don't cause
        // a ConcurrencyException due to the same sitemap document being updated.

        var timeout = TimeSpan.FromMilliseconds(20_000);
        (var locker, var locked) = await _distributedLock.TryAcquireLockAsync("SITEMAPS_UPDATE_LOCK", timeout, timeout);

        if (!locked)
        {
            throw new TimeoutException($"Couldn't acquire a lock to update the sitemap within {timeout.Seconds} seconds.");
        }

        using (locker)
        {
            foreach (var sitemapTypeUpdateHandler in _sitemapTypeUpdateHandlers)
            {
                await sitemapTypeUpdateHandler.UpdateSitemapAsync(context);
            }
        }
    }
}
