using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;

namespace Orchard.Hosting.Middleware
{
    /// <summary>
    /// This middleware will execute the standard ASP.NET MVC pipeline, as it marks the end of
    /// the tenant specific pipeline
    /// </summary>
    public static class OrchardMiddleware
    {
        public static IApplicationBuilder UseOrchard(this IApplicationBuilder app)
        {
            app.Use(async (context, task) =>
            {
                var handler = context.Items["orchard.Handler"] as Func<Task>;

                var target = context.Items["orchard.Handler.Target"] as IRouter;
                var routeContext = context.Items["orchard.Handler.RouteContext"] as RouteContext;

                if (target == null)
                {
                    throw new ArgumentException("orchard.Handler can't be null");
                }

                await target.RouteAsync(routeContext);
            });

            return app;
        }
    }
}