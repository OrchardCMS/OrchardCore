using System.Threading.RateLimiting;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Http;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OrchardCore.RateLimits;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OrchardCore.OpenId.Handlers;

/// <summary>
/// Limits anonymous device authorization requests per remote address. The OpenID server answers them itself from the
/// authentication middleware, so they reach neither a route nor the rate limiter middleware, which runs after it.
/// </summary>
public sealed class DeviceAuthorizationRateLimitHandler : IOpenIddictServerHandler<ValidateDeviceAuthorizationRequestContext>, IDisposable
{
    private const int PermitLimit = 30;

    private readonly PartitionedRateLimiter<HttpContext> _limiter = PartitionedRateLimiter.Create(
        RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy("OpenIdDeviceAuthorization", PermitLimit));

    /// <inheritdoc />
    public ValueTask HandleAsync(ValidateDeviceAuthorizationRequestContext context)
    {
        var httpContext = context.Transaction.GetHttpRequest()?.HttpContext;
        if (httpContext is null)
        {
            return ValueTask.CompletedTask;
        }

        using var lease = _limiter.AttemptAcquire(httpContext);
        if (!lease.IsAcquired)
        {
            // A sliding window limiter doesn't tell when a permit is available, so this is its window.
            httpContext.Response.Headers.RetryAfter = "60";

            // The error description is not localized as OAuth 2.0 only allows select US-ASCII characters in it.
            context.Reject(OpenIddictConstants.Errors.SlowDown, "Too many requests. Please retry later.");
        }

        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose() => _limiter.Dispose();
}
