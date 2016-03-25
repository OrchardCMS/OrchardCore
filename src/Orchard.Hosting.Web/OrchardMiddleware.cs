using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Hosting
{
    public class OrchardMiddleware
    {
        private readonly RequestDelegate _next;

        public OrchardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // There is no further middleware to invoke, Orchard is the last one

            var context = httpContext.Items["orchard.middleware.context"] as RouteContext;
            var routes = httpContext.Items["orchard.middleware.routes"] as IEnumerable<IRouter>;

            // Process each route in order        
            foreach (var route in routes)
            {
                await route.RouteAsync(context);

                if (context.Handler != null)
                {
                    return;
                }
            }
        }
    }
}
