using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace OrchardCore.Security.Middlewares
{
    public class PermissionsPolicyMiddleware
    {
        private readonly PermissionsPolicyOptions _options;
        private readonly RequestDelegate _next;

        public PermissionsPolicyMiddleware(IOptions<PermissionsPolicyOptions> options, RequestDelegate next)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _options = options.Value;
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = _options.ToString();

            return _next.Invoke(context);
        }
    }
}
