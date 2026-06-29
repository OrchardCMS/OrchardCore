using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.Modules;

/// <summary>
/// Adds the X-Powered-By header with values OrchardCore.
/// </summary>
public class PoweredByMiddleware
{
    private readonly RequestDelegate _next;
    private readonly PoweredByOptions _poweredByOptions;

    public PoweredByMiddleware(RequestDelegate next, IOptions<PoweredByOptions> poweredByOptions)
    {
        _next = next;
        _poweredByOptions = poweredByOptions.Value;
    }

    public Task Invoke(HttpContext httpContext)
    {
        if (_poweredByOptions.Enabled)
        {
            httpContext.Response.Headers[_poweredByOptions.HeaderName] = _poweredByOptions.HeaderValue;
        }

        return _next.Invoke(httpContext);
    }
}

[Obsolete("This interface is deprecated.", error: true)]
public interface IPoweredByMiddlewareOptions
{
    bool Enabled { get; set; }
    string HeaderName { get; }
    string HeaderValue { get; set; }
}

[Obsolete("This class is deprecated. Use PoweredByOptions instead.", error: true)]
internal sealed class PoweredByMiddlewareOptions : IPoweredByMiddlewareOptions
{
    private const string PoweredByHeaderName = "X-Powered-By";
    private const string PoweredByHeaderValue = "OrchardCore";

    public string HeaderName => PoweredByHeaderName;
    public string HeaderValue { get; set; } = PoweredByHeaderValue;

    public bool Enabled { get; set; } = true;
}
