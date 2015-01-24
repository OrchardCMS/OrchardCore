using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Routing {
    public class OrchardRouterMiddleware {
        private readonly RequestDelegate _next;

        public OrchardRouterMiddleware(
            RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext) {
            Console.WriteLine("Begin Routing Request");

            var router = httpContext.RequestServices.GetService<IRouteBuilder>().Build();

            var context = new RouteContext(httpContext);
            context.RouteData.Routers.Add(router);

            await router.RouteAsync(context);

            if (!context.IsHandled) {
                await _next.Invoke(httpContext);
            }

            Console.WriteLine("End Routing Request");
        }
    }
}