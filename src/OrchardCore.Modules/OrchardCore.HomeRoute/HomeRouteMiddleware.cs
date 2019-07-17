using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute
{
    public class HomeRouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ISiteService _siteService;
        private IChangeToken _siteServicechangeToken;
        private volatile RouteValueDictionary _homeRoute;

        public HomeRouteMiddleware(RequestDelegate next, ISiteService siteService)
        {
            _next = next;
            _siteService = siteService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_siteServicechangeToken?.HasChanged ?? true)
            {
                _homeRoute = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
                _siteServicechangeToken = _siteService.ChangeToken;
            }

            httpContext.Features.Set(new HomeRouteFeature
            {
                HomeRoute = _homeRoute
            });

            await _next.Invoke(httpContext);
        }
    }
}
