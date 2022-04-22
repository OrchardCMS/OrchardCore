using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public class SecurityHeadersMiddleware
    {
        private readonly SecurityHeadersOptions _options;
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(IOptions<SecurityHeadersOptions> options, RequestDelegate next)
        {
            _options = options.Value;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeaderNames.XContentTypeOptions] = _options.ContentTypeOptions.Value;
            context.Response.Headers[SecurityHeaderNames.XFrameOptions] = _options.FrameOptions.Value;
            context.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = _options.PermissionsPolicy.ToString();
            context.Response.Headers[SecurityHeaderNames.ReferrerPolicy] = _options.ReferrerPolicy.Value;

            return _next.Invoke(context);
        }
    }
}
