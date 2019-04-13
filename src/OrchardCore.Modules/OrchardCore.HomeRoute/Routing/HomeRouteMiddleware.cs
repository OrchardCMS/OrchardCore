using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HomePageRoute _homePageRoute;
        private readonly LinkGenerator _linkGenerator;

        public HomeRouteMiddleware(RequestDelegate next, HomePageRoute homePageRoute, LinkGenerator linkGenerator)
        {
            _next = next;
            _homePageRoute = homePageRoute;
            _linkGenerator = linkGenerator;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path.ToString().TrimEnd('/') == String.Empty)
            {
                var routeValues = await _homePageRoute.GetValuesAsync();

                if (routeValues != null)
                {
                    httpContext.Request.Path = _linkGenerator.GetPathByRouteValues(null, routeValues);
                }
            }

            await _next.Invoke(httpContext);
        }
    }
}

