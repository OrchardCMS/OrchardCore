using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.ReverseProxy.Services;

/// <summary>
/// Applies the forwarded headers with the current <see cref="ForwardedHeadersOptions"/>, as the
/// <see cref="ForwardedHeadersMiddleware"/> copies the options it is built with.
/// </summary>
internal sealed class ReverseProxyForwardedHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IOptionsMonitor<ForwardedHeadersOptions> _forwardedHeadersOptions;

    private ForwardedHeadersHandler _handler;

    public ReverseProxyForwardedHeadersMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory,
        IOptionsMonitor<ForwardedHeadersOptions> forwardedHeadersOptions)
    {
        _next = next;
        _loggerFactory = loggerFactory;
        _forwardedHeadersOptions = forwardedHeadersOptions;
    }

    public Task Invoke(HttpContext context)
        => GetMiddleware().Invoke(context);

    private ForwardedHeadersMiddleware GetMiddleware()
    {
        var options = _forwardedHeadersOptions.CurrentValue;

        var handler = _handler;
        if (ReferenceEquals(handler?.Options, options))
        {
            return handler.Middleware;
        }

        var middleware = new ForwardedHeadersMiddleware(_next, _loggerFactory, Microsoft.Extensions.Options.Options.Create(options));

        _handler = new ForwardedHeadersHandler(options, middleware);

        return middleware;
    }

    private sealed record ForwardedHeadersHandler(ForwardedHeadersOptions Options, ForwardedHeadersMiddleware Middleware);
}
