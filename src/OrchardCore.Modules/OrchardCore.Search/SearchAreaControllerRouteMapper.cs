using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Entities;
using OrchardCore.Mvc.Routing;
using OrchardCore.Search.Model;
using OrchardCore.Settings;

namespace OrchardCore.Search
{
    public class SearchAreaControllerRouteMapper : IAreaControllerRouteMapper
    {
        private readonly ISiteService _site;

        public int Order => -1000;

        public SearchAreaControllerRouteMapper(ISiteService site)
        {
            _site = site;
        }

        public bool TryMapAreaControllerRoute(IEndpointRouteBuilder routes, ControllerActionDescriptor descriptor)
        {
            var settings = _site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SearchSettings>();

            if (!String.IsNullOrEmpty(settings.SearchProvider))
            {
                routes.MapAreaControllerRoute(
                    name: "Search",
                    areaName: "OrchardCore.Search." + settings.SearchProvider,
                    pattern: "Search",
                    defaults: new { controller = "Search", action = "Search" }
                );
            }

            return true;
        }
    }
}
