using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;

namespace OrchardVNext.Middleware
{
    /// <summary>
    /// A special Owin middleware that is executed last in the Owin pipeline and runs the non-Owin part of the request.
    /// </summary>
    public static class OrchardMiddleware {
        public static IApplicationBuilder UseOrchard(this IApplicationBuilder app) {
            app.Use(async (context, task) => {
                var handler = context.Items["orchard.Handler"] as Func<Task>;

                if (handler == null) {
                    throw new ArgumentException("orchard.Handler can't be null");
                }
                await handler();
            });

            return app;
        }
    }
}