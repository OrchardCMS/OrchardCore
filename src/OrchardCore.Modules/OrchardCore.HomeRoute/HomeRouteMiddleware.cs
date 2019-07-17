using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.HomeRoute
{
    public class HomeRouteMiddleware
    {
        private readonly RequestDelegate _next;

        public HomeRouteMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var siteService = httpContext.RequestServices.GetService<ISiteService>();

            if (siteService != null)
            {
                var homeRoute = (await siteService.GetSiteSettingsAsync()).HomeRoute;

                httpContext.Features.Set(new HomeRouteFeature
                {
                    HomeRoute = homeRoute
                });
            }

            await _next.Invoke(httpContext);
        }
    }
}
