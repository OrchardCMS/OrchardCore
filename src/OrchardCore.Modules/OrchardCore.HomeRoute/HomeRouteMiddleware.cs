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
        private RouteValueDictionary _homeRoute;
        private IChangeToken _changeToken;

        public HomeRouteMiddleware(RequestDelegate next, ISiteService siteService)
        {
            _next = next;
            _siteService = siteService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_changeToken?.HasChanged ?? true)
            {
                _changeToken = _siteService.ChangeToken;
                _homeRoute = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
            }

            httpContext.Features.Set(new HomeRouteFeature
            {
                HomeRoute = _homeRoute
            });

            await _next.Invoke(httpContext);
        }
    }
}
