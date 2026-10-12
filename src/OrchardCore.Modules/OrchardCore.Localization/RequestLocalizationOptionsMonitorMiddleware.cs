using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization;

/// <summary>
/// Sets the request culture like <see cref="RequestLocalizationMiddleware"/>, but from the current value of
/// <see cref="IOptionsMonitor{TOptions}"/> so that updated <see cref="RequestLocalizationOptions"/> apply
/// without rebuilding the tenant pipeline.
/// </summary>
/// <remarks>
/// <see cref="RequestLocalizationMiddleware"/> copies its options when it is created, so an instance is created
/// and reused for each new <see cref="RequestLocalizationOptions"/> value.
/// </remarks>
public sealed class RequestLocalizationOptionsMonitorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<RequestLocalizationOptions> _optionsMonitor;
    private readonly ILoggerFactory _loggerFactory;

    private CachedMiddleware _cachedMiddleware;

    /// <summary>
    /// Creates a new instance of <see cref="RequestLocalizationOptionsMonitorMiddleware"/>.
    /// </summary>
    /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
    /// <param name="optionsMonitor">The <see cref="IOptionsMonitor{TOptions}"/> of the <see cref="RequestLocalizationOptions"/>.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
    public RequestLocalizationOptionsMonitorMiddleware(
        RequestDelegate next,
        IOptionsMonitor<RequestLocalizationOptions> optionsMonitor,
        ILoggerFactory loggerFactory)
    {
        _next = next;
        _optionsMonitor = optionsMonitor;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Invokes the logic of the middleware.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/>.</param>
    public Task InvokeAsync(HttpContext context)
    {
        var options = _optionsMonitor.CurrentValue;
        var cachedMiddleware = _cachedMiddleware;

        if (cachedMiddleware is null || !ReferenceEquals(cachedMiddleware.Options, options))
        {
            cachedMiddleware = new CachedMiddleware(options, new RequestLocalizationMiddleware(_next, Options.Create(options), _loggerFactory));
            _cachedMiddleware = cachedMiddleware;
        }

        return cachedMiddleware.Middleware.Invoke(context);
    }

    private sealed record CachedMiddleware(RequestLocalizationOptions Options, RequestLocalizationMiddleware Middleware);
}
