using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Routing;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteRoutingFilter : IShellRoutingFilter
    {
        private readonly HomeRoute _homeRoute;
        private readonly LinkGenerator _linkGenerator;

        public HomeRouteRoutingFilter(HomeRoute homeRoute, LinkGenerator linkGenerator)
        {
            _homeRoute = homeRoute;
            _linkGenerator = linkGenerator;
        }

        public async Task OnRoutingAsync(HttpContext httpContext)
        {
            if (httpContext.Request.Path.ToString().TrimEnd('/') == String.Empty)
            {
                var routeValues = await _homeRoute.GetValuesAsync();

                if (routeValues != null)
                {
                    httpContext.Request.Path = _linkGenerator.GetPathByRouteValues(null, routeValues);
                }
            }
        }
    }
}

