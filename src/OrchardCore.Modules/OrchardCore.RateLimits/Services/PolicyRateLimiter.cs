using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.RateLimits.Models;

namespace OrchardCore.RateLimits.Services;

/// <summary>
/// Applies the limiters of the current enabled policies, and replaces them when the policies change,
/// as the rate-limiting middleware keeps the global limiter it is built with.
/// </summary>
internal sealed class PolicyRateLimiter : PartitionedRateLimiter<HttpContext>
{
    private readonly IOptionsMonitor<RateLimitPoliciesOptions> _policiesOptions;
    private readonly Func<IReadOnlyCollection<RateLimitPolicy>, IReadOnlyList<PartitionedRateLimiter<HttpContext>>> _limitersFactory;
    private readonly Lock _lock = new();

    private PolicyLimiters _current;
    private bool _disposed;

    public PolicyRateLimiter(
        IOptionsMonitor<RateLimitPoliciesOptions> policiesOptions,
        Func<IReadOnlyCollection<RateLimitPolicy>, IReadOnlyList<PartitionedRateLimiter<HttpContext>>> limitersFactory)
    {
        _policiesOptions = policiesOptions;
        _limitersFactory = limitersFactory;
    }

    public override RateLimiterStatistics GetStatistics(HttpContext resource)
        => GetLimiters().Limiter.GetStatistics(resource);

    protected override RateLimitLease AttemptAcquireCore(HttpContext resource, int permitCount)
    {
        while (true)
        {
            var limiters = GetLimiters();

            try
            {
                return limiters.Limiter.AttemptAcquire(resource, permitCount);
            }
            catch (ObjectDisposedException) when (IsReplaced(limiters))
            {
                // The limiters were replaced and disposed concurrently, retry with the new ones.
            }
        }
    }

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(HttpContext resource, int permitCount, CancellationToken cancellationToken)
    {
        while (true)
        {
            var limiters = GetLimiters();

            try
            {
                return await limiters.Limiter.AcquireAsync(resource, permitCount, cancellationToken);
            }
            catch (ObjectDisposedException) when (IsReplaced(limiters))
            {
                // The limiters were replaced and disposed concurrently, retry with the new ones.
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        PolicyLimiters current;

        lock (_lock)
        {
            _disposed = true;
            current = _current;
            _current = null;
        }

        current?.Dispose();
    }

    private PolicyLimiters GetLimiters()
    {
        var options = _policiesOptions.CurrentValue;

        var current = _current;
        if (ReferenceEquals(current?.Options, options))
        {
            return current;
        }

        PolicyLimiters replaced;

        lock (_lock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            current = _current;
            if (ReferenceEquals(current?.Options, options))
            {
                return current;
            }

            replaced = current;
            current = new PolicyLimiters(options, _limitersFactory(options.EnabledPolicies));

            _current = current;
        }

        // Dispose the limiters of the previous policies, so that their timers are released.
        replaced?.Dispose();

        return current;
    }

    private bool IsReplaced(PolicyLimiters limiters)
        => !ReferenceEquals(_current, limiters) && !_disposed;

    private sealed class PolicyLimiters : IDisposable
    {
        private readonly IReadOnlyList<PartitionedRateLimiter<HttpContext>> _limiters;

        public PolicyLimiters(RateLimitPoliciesOptions options, IReadOnlyList<PartitionedRateLimiter<HttpContext>> limiters)
        {
            Options = options;
            _limiters = limiters;

            Limiter = limiters.Count switch
            {
                0 => PartitionedRateLimiter.Create<HttpContext, string>(_ => RateLimitPartition.GetNoLimiter("policies")),
                1 => limiters[0],
                _ => PartitionedRateLimiter.CreateChained([.. limiters]),
            };
        }

        public RateLimitPoliciesOptions Options { get; }

        public PartitionedRateLimiter<HttpContext> Limiter { get; }

        public void Dispose()
        {
            // A chained limiter doesn't dispose the limiters it chains.
            if (_limiters.Count != 1)
            {
                Limiter.Dispose();
            }

            foreach (var limiter in _limiters)
            {
                limiter.Dispose();
            }
        }
    }
}
