using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapsTransformer : DynamicRouteValueTransformer
    {
        private readonly SitemapEntries _entries;
        private readonly SitemapsOptions _options;

        public SitemapsTransformer(SitemapEntries entries, IOptions<SitemapsOptions> options)
        {
            _entries = entries;
            _options = options.Value;
        }

        public override ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // Use route value provided by SitemapTransformer template.
            var path = values["sitemap"] as string;
            if (!String.IsNullOrEmpty(path) && _entries.TryGetSitemapIdByPath(path, out var sitemapId))
            {
                var routeValues = new RouteValueDictionary(_options.GlobalRouteValues)
                {
                    [_options.SitemapIdKey] = sitemapId
                };

                return new ValueTask<RouteValueDictionary>(routeValues);
            }

            return new ValueTask<RouteValueDictionary>((RouteValueDictionary)null);
        }
    }
}
