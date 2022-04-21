using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Middlewares
{
    public class ReferrerPolicyMiddleware
    {
        private readonly ReferrerPolicyOptions _options;
        private readonly RequestDelegate _next;

        public ReferrerPolicyMiddleware(IOptions<ReferrerPolicyOptions> options, RequestDelegate next)
        {
            _options = options.Value;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeaderNames.ReferrerPolicy] = _options.Value;

            return _next.Invoke(context);
        }
    }
}
