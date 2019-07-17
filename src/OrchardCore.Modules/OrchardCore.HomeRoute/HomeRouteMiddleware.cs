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

        // 'HomeRoute' requires a registered 'ISiteService' implementation.
        public HomeRouteMiddleware(RequestDelegate next, ISiteService siteService)
        {
            _next = next;
            _siteService = siteService;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_changeToken?.HasChanged ?? true)
            {
                // Assumed to be atomic as we just update reference types.
                _homeRoute = (await _siteService.GetSiteSettingsAsync()).HomeRoute;
                _changeToken = _siteService.ChangeToken;
            }

            httpContext.Features.Set(new HomeRouteFeature
            {
                HomeRoute = _homeRoute
            });

            await _next.Invoke(httpContext);
        }
    }
}
