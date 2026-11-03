using System.Threading.RateLimiting;

namespace OrchardCore.RateLimits;

/// <summary>
/// A sliding-window limiter telling when to retry, which <see cref="SlidingWindowRateLimiter"/> doesn't. The window
/// is the time after which the permits in use are back, whichever segments they were taken from.
/// </summary>
internal sealed class SlidingWindowRetryAfterRateLimiter : ReplenishingRateLimiter
{
    private readonly SlidingWindowRateLimiter _limiter;
    private readonly TimeSpan _window;

    public SlidingWindowRetryAfterRateLimiter(SlidingWindowRateLimiterOptions options)
    {
        _limiter = new SlidingWindowRateLimiter(options);
        _window = options.Window;
    }

    public override TimeSpan? IdleDuration => _limiter.IdleDuration;

    public override bool IsAutoReplenishing => _limiter.IsAutoReplenishing;

    public override TimeSpan ReplenishmentPeriod => _limiter.ReplenishmentPeriod;

    public override bool TryReplenish() => _limiter.TryReplenish();

    public override RateLimiterStatistics GetStatistics() => _limiter.GetStatistics();

    protected override RateLimitLease AttemptAcquireCore(int permitCount) => WithRetryAfter(_limiter.AttemptAcquire(permitCount));

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken) =>
        WithRetryAfter(await _limiter.AcquireAsync(permitCount, cancellationToken));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _limiter.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await _limiter.DisposeAsync();
        await base.DisposeAsyncCore();
    }

    private RateLimitLease WithRetryAfter(RateLimitLease lease)
    {
        if (lease.IsAcquired)
        {
            return lease;
        }

        lease.Dispose();

        return new RejectedLease(_window);
    }

    private sealed class RejectedLease : RateLimitLease
    {
        private readonly TimeSpan _retryAfter;

        public RejectedLease(TimeSpan retryAfter)
        {
            _retryAfter = retryAfter;
        }

        public override bool IsAcquired => false;

        public override IEnumerable<string> MetadataNames => [MetadataName.RetryAfter.Name];

        public override bool TryGetMetadata(string metadataName, out object metadata)
        {
            if (metadataName == MetadataName.RetryAfter.Name)
            {
                metadata = _retryAfter;

                return true;
            }

            metadata = null;

            return false;
        }
    }
}
