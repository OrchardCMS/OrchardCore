using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Routing
{
    public class ShellRoutingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnumerable<IShellRoutingFilter> _filters;

        public ShellRoutingMiddleware(RequestDelegate next, IEnumerable<IShellRoutingFilter> filters)
        {
            _next = next;
            _filters = filters;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            foreach (var filter in _filters)
            {
                await filter.OnRoutingAsync(httpContext);
            }

            await _next.Invoke(httpContext);
        }
    }
}
