using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Entities;
using OrchardCore.Search.Abstractions;
using OrchardCore.Search.Model;
using OrchardCore.Settings;

namespace OrchardCore.Search.Routing
{
    public class SearchRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly ISiteService _siteService;
        private readonly IServiceProvider _serviceProvider;

        public SearchRouteTransformer(
            ISiteService siteService,
            IServiceProvider serviceProvider)
        {
            _siteService = siteService;
            _serviceProvider = serviceProvider;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            var searchProviders = _serviceProvider.GetServices<SearchProvider>();
            RouteValueDictionary routeValue = null;

            if (searchProviders.Count() == 1)
            {
                routeValue = new RouteValueDictionary(new {
                    Name = "Search",
                    Area = searchProviders.FirstOrDefault().AreaName,
                    Action = "Search",
                    Controller = "Search" }
                );
            }
            else
            {
                var site = await _siteService.GetSiteSettingsAsync();
                var settings = site.As<SearchSettings>();

                if (!String.IsNullOrEmpty(settings.SearchProviderAreaName))
                {
                    routeValue = new RouteValueDictionary(new
                    {
                        Name = "Search",
                        Area = settings.SearchProviderAreaName,
                        Action = "Search",
                        Controller = "Search"
                    });
                }
            }

            return routeValue;
        }
    }
}
