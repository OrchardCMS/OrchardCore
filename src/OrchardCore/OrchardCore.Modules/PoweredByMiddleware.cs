using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Adds the X-Powered-By header with values OrchardCore.
    /// </summary>
    internal class PoweredByMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPoweredByMiddlewareOptions _options;

        public PoweredByMiddleware(RequestDelegate next, IPoweredByMiddlewareOptions options)
        {
            _next = next;
            _options = options;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_options.Enabled)
                AddHeader(httpContext, _options.HeaderName, _options.HeaderValue);
            await _next.Invoke(httpContext);
        }

        /// <summary>
        /// Adds a delegate to responses OnStarting event in order to add
        /// a header with <paramref name="headerName"/> and value <paramref name="headerValue"/>
        /// </summary>
        /// <param name="httpContext">The Http Context</param>
        /// <param name="headerName">Header name</param>
        /// <param name="headerValue">Header value</param>
        private static void AddHeader(HttpContext httpContext, string headerName, string headerValue)
        {
            httpContext.Response.OnStarting(() =>
            {
                var headers = httpContext.Response.Headers;

                if (!headers.ContainsKey(headerName))
                    headers.Add(headerName, headerValue);
                else headers.Append(headerName, headerValue);

                return Task.CompletedTask;
            });
        }
    }
    
    public interface IPoweredByMiddlewareOptions
    {
        bool Enabled { get; set; }
        string HeaderName { get; }
        string HeaderValue { get; set; }
    }

    class PoweredByMiddlewareOptions
        : IPoweredByMiddlewareOptions
    {
        const string PoweredByHeaderName = "X-Powered-By";
        const string PoweredByHeaderValue = "OrchardCore";

        public string HeaderName => PoweredByHeaderName;
        public string HeaderValue { get; set; } = PoweredByHeaderValue;

        public bool Enabled { get; set; } = true;
    }
}
