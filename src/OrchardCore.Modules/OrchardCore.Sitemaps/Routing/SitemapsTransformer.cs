using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapsTransformer : DynamicRouteValueTransformer
    {
        public const string RouteKey = "sitemap";
        private readonly SitemapEntries _entries;

        public SitemapsTransformer(SitemapEntries entries)
        {
            _entries = entries;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            //TODO resolve /

            // Need to check path keys conventions, here they never start with a '/' but
            // e.g autoroute path keys always have a leading '/' but never a trailing one.
            if (!String.IsNullOrEmpty(values[RouteKey]?.ToString()) &&
                _entries.TryGetSitemapId(httpContext.Request.Path.Value.Trim('/'), out var sitemapNodeId))
            {
                var routeValues = new RouteValueDictionary();

                // TODO Quickly done, could be from an IOptions as in 'AutoRouteTransformer'
                routeValues.Add("area", "OrchardCore.Sitemaps");
                routeValues.Add("controller", "Sitemap");
                routeValues.Add("action", "Index");

                routeValues.Add(RouteKey, values[RouteKey]);

                return new ValueTask<RouteValueDictionary>(routeValues);
            }

            return new ValueTask<RouteValueDictionary>((RouteValueDictionary)null);
        }
    }
}
