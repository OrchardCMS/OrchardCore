using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Autoroute.Services;
using OrchardCore.Routing;

namespace OrchardCore.Autoroute.Routing
{
    public class AutoRouteRoutingFilter : IShellRoutingFilter
    {
        private readonly IAutorouteEntries _entries;
        private readonly LinkGenerator _linkGenerator;

        public AutoRouteRoutingFilter(IAutorouteEntries entries, LinkGenerator linkGenerator)
        {
            _entries = entries;
            _linkGenerator = linkGenerator;
        }

        public async Task OnRoutingAsync(HttpContext httpContext)
        {
            if (_entries.TryGetContentItemId(httpContext.Request.Path.ToString().TrimEnd('/'), out var contentItemId))
            {
                var autoRoute = httpContext.RequestServices.GetRequiredService<AutoRoute>();
                var routeValues = await autoRoute.GetValuesAsync(contentItemId);

                if (routeValues != null)
                {
                    httpContext.Request.Path = _linkGenerator.GetPathByRouteValues(null, routeValues);
                }
            }
        }
    }
}
