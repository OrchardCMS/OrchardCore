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
            _options = options;
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeader.ReferrerPolicy] = _options;

            return _next.Invoke(context);
        }
    }
}
