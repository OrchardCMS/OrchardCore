using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Hosting.Web.Routing {
    public class OrchardRouterMiddleware {
        private readonly RequestDelegate _next;

        public OrchardRouterMiddleware(
            RequestDelegate next) {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext) {
            Logger.Information("Begin Routing Request");

            var router = httpContext.RequestServices.GetService<IRouteBuilder>().Build();

            var context = new RouteContext(httpContext);
            context.RouteData.Routers.Add(router);

            await router.RouteAsync(context);

            if (!context.IsHandled) {
                Logger.Information("Request did not match any routes.");
                await _next.Invoke(httpContext);
            }

            Logger.Information("End Routing Request");
        }
    }
}