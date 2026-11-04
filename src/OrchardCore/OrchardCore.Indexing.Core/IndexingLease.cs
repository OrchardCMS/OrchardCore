using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Indexing.Core;

// Measure conservatively from before acquisition; the lock API has no renewal or
// ownership token. Expiry stops new work, but cannot cancel an in-flight provider call.
internal sealed class IndexingLease
{
    internal static readonly TimeSpan Duration = TimeSpan.FromMinutes(15);
    private readonly TimeProvider _time;
    private readonly long _started;

    internal IndexingLease(IServiceProvider services)
    {
        _time = services.GetService<TimeProvider>() ?? TimeProvider.System;
        _started = _time.GetTimestamp();
    }

    internal bool Expired => _time.GetElapsedTime(_started) >= Duration;

    internal void EnsureActive()
    {
        if (Expired) { throw new IndexingLeaseExpiredException(); }
    }
}

internal sealed class IndexingLeaseExpiredException : Exception;
