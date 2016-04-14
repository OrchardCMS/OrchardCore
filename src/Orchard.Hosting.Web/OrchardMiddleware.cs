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
            // Same logic as in https://github.com/aspnet/Routing/blob/dev/src/Microsoft.AspNetCore.Routing/RouterMiddleware.cs

            var context = httpContext.Items["orchard.middleware.context"] as RouteContext;
            var routes = httpContext.Items["orchard.middleware.routes"] as IEnumerable<IRouter>;

            // Process each route in order
            foreach (var route in routes)
            {
                await route.RouteAsync(context);

                if (context.Handler != null)
                {
                    httpContext.Features[typeof(IRoutingFeature)] = new RoutingFeature()
                    {
                        RouteData = context.RouteData,
                    };

                    await context.Handler(context.HttpContext);

                    return;
                }
            }

            // Request did not match this route
            await _next.Invoke(httpContext);
        }

        private class RoutingFeature : IRoutingFeature
        {
            public RouteData RouteData { get; set; }
        }
    }
}
