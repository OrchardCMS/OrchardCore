using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Middlewares
{
    public class FrameOptionsMiddleware
    {
        private readonly FrameOptionsOptions _options;
        private readonly RequestDelegate _next;

        public FrameOptionsMiddleware(IOptions<FrameOptionsOptions> options, RequestDelegate next)
        {
            _options = options.Value;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeaderNames.XFrameOptions] = _options.Value;

            return _next.Invoke(context);
        }
    }
}
