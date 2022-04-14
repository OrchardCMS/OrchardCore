using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Security.Middlewares
{
    public class ReferrerPolicyMiddleware
    {
        private readonly ReferrerPolicy _policy;
        private readonly RequestDelegate _next;

        public ReferrerPolicyMiddleware(ReferrerPolicy policy, RequestDelegate next)
        {
            _policy = policy;
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            context.Response.Headers[SecurityHeader.ReferrerPolicy] = _policy;

            return _next.Invoke(context);
        }
    }
}
