using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.Web.Routing
{
    public class OrchardRouterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IRouter> _routers = new ConcurrentDictionary<string, IRouter>();
        
        public OrchardRouterMiddleware(
            RequestDelegate next,
            ILogger<OrchardRouterMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Begin Routing Request");
            }

            var shellSettings = httpContext.RequestServices.GetService<ShellSettings>();

            var router = _routers.GetOrAdd(
                shellSettings.Name, 
                name => httpContext.RequestServices.GetService<IRouteBuilder>().Build()
            );

            var context = new RouteContext(httpContext);
            context.RouteData.Routers.Add(router);

            await router.RouteAsync(context);

            if (!context.IsHandled)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Request did not match any routes.");
                }

                await _next.Invoke(httpContext);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("End Routing Request");
            }
        }
    }
}