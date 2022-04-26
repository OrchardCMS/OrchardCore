using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Represents a middleware that Adds X-Powered-By HTTP header with the value <c>Orchard Core</c>.
    /// </summary>
    public class PoweredByMiddleware
    {
        private readonly PoweredByOptions _options;
        private readonly RequestDelegate _next;

        /// <summary>
        /// Creates an instance of <see cref="PoweredByMiddleware"/>.
        /// </summary>
        /// <param name="options">The <see cref="IOptions{TOptions}"/>.</param>
        /// <param name="next">The <see cref="RequestDelegate"/>.</param>
        public PoweredByMiddleware(IOptions<PoweredByOptions> options, RequestDelegate next)
        {
            _options = options.Value;
            _next = next;
        }

        /// <inheritdoc/>
        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.Headers.XPoweredBy = _options.Value;

            return _next.Invoke(httpContext);
        }
    }
}
