using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Middlewares
{
    public class ReferrerPolicyMiddleware
    {
        private readonly string _options;
        private readonly RequestDelegate _next;

        public ReferrerPolicyMiddleware(string options, RequestDelegate next)
        {
            if (String.IsNullOrEmpty(options))
            {
                throw new ArgumentException($"'{nameof(options)}' cannot be null or empty.", nameof(options));
            }

            _options = options;
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeaderNames.ReferrerPolicy] = _options;

            return _next.Invoke(context);
        }
    }
}
