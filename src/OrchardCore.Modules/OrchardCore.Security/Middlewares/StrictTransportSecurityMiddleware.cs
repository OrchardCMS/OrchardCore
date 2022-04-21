using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Middlewares
{
    public class StrictTransportSecurityMiddleware
    {
        private readonly StrictTransportSecurityOptions _options;
        private readonly RequestDelegate _next;

        public StrictTransportSecurityMiddleware(IOptions<StrictTransportSecurityOptions> options, RequestDelegate next)
        {
            _options = options.Value;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var value = "max-age=" + _options.MaxAge.TotalSeconds;
            if (_options.IncludeSubDomains)
            {
                value += "; includeSubDomains";
            }

            if (_options.Preload)
            {
                value += "; preload";
            }

            context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity] = value;

            return _next.Invoke(context);
        }
    }
}
