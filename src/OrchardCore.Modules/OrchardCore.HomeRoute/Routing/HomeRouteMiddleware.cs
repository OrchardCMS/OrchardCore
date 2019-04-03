using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute.Routing
{
    public class HomeRouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISiteService _siteService;
        private readonly LinkGenerator _linkGenerator;
        private RouteValueDictionary _homeRoute;
        private IChangeToken _siteServicechangeToken;

        public HomeRouteMiddleware(RequestDelegate next, ISiteService siteService, LinkGenerator linkGenerator)
        {
            _next = next;
            _siteService = siteService;
            _linkGenerator = linkGenerator;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Request.Path == "/")
            {
                var homeRoutes = GetHomeRouteValues(httpContext);

                if (homeRoutes != null)
                {
                    httpContext.Request.Path = _linkGenerator.GetPathByRouteValues(null, homeRoutes);
                }
            }

            await _next.Invoke(httpContext);
        }

        private RouteValueDictionary GetHomeRouteValues(HttpContext httpContext)
        {
            if (_siteServicechangeToken == null || _siteServicechangeToken.HasChanged)
            {
                _homeRoute = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult().HomeRoute;
                _siteServicechangeToken = _siteService.ChangeToken;
            }

            return _homeRoute;
        }
    }
}

