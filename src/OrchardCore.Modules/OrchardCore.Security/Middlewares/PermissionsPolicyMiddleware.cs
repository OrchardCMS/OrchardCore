using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Middlewares
{
    public class PermissionsPolicyMiddleware
    {
        private readonly IEnumerable<string> _options;
        private readonly RequestDelegate _next;

        public PermissionsPolicyMiddleware(IEnumerable<string> options, RequestDelegate next)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeaderNames.PermissionsPolicy] = String.Join(',', _options.Select(o => $"{o}=(*)"));

            return _next.Invoke(context);
        }
    }
}
