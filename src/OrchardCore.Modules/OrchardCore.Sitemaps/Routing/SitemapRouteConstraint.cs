using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapRouteConstraint : IRouteConstraint
    {
        public const string RouteKey = "sitemaps";

        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var sitemapPath = values[routeKey]?.ToString();

            if (String.IsNullOrEmpty(sitemapPath))
            {
                return false;
            }

            var _sitemapEntries = httpContext.RequestServices.GetService<SitemapEntries>();

            return _sitemapEntries.TryGetSitemapNodeId(sitemapPath, out var sitemapNodeId);
        }
    }
}
