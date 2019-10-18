using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapsTransformer : DynamicRouteValueTransformer
    {
        private readonly SitemapEntries _entries;
        private readonly SitemapOptions _options;

        public SitemapsTransformer(SitemapEntries entries, IOptions<SitemapOptions> options)
        {
            _entries = entries;
            _options = options.Value;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // Use route value provided by SitemapTransformer template.
            if (_entries.TryGetSitemapId(values["sitemap"] as string, out var sitemapId))
            {
                var routeValues = new RouteValueDictionary(_options.GlobalRouteValues);
                routeValues[_options.SitemapIdKey] = sitemapId;

                return new ValueTask<RouteValueDictionary>(routeValues);
            }

            return new ValueTask<RouteValueDictionary>((RouteValueDictionary)null);
        }
    }
}
