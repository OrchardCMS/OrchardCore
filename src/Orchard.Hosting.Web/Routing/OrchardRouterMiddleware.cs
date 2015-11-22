using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Orchard.Hosting.Web.Routing
{
    public class OrchardRouterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public OrchardRouterMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<OrchardRouterMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            _logger.LogInformation("Begin Routing Request");

            var router = httpContext.RequestServices.GetService<IRouteBuilder>().Build();

            var context = new RouteContext(httpContext);
            context.RouteData.Routers.Add(router);

            await router.RouteAsync(context);

            if (!context.IsHandled)
            {
                _logger.LogInformation("Request did not match any routes.");
                await _next.Invoke(httpContext);
            }

            _logger.LogInformation("End Routing Request");
        }
    }
}