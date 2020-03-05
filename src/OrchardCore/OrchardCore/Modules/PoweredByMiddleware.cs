using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Adds the X-Powered-By header with values OrchardCore.
    /// </summary>
    public class PoweredByMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPoweredByMiddlewareOptions _options;

        public PoweredByMiddleware(RequestDelegate next, IPoweredByMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public Task Invoke(HttpContext httpContext)
        {
            if (_options.Enabled)
            {
                httpContext.Response.Headers[_options.HeaderName] = _options.HeaderValue;
            }

            return _next.Invoke(httpContext);
        }
    }

    public interface IPoweredByMiddlewareOptions
    {
        bool Enabled { get; set; }
        string HeaderName { get; }
        string HeaderValue { get; set; }
    }

    internal class PoweredByMiddlewareOptions : IPoweredByMiddlewareOptions
    {
        private const string PoweredByHeaderName = "X-Powered-By";
        private const string PoweredByHeaderValue = "OrchardCore";

        public string HeaderName => PoweredByHeaderName;
        public string HeaderValue { get; set; } = PoweredByHeaderValue;

        public bool Enabled { get; set; } = true;
    }
}
