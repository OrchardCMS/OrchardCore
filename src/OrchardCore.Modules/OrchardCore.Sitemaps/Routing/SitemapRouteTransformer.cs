using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly SitemapEntries _sitemapEntries;
        private readonly SitemapsOptions _options;

        public SitemapRouteTransformer(SitemapEntries sitemapEntries, IOptions<SitemapsOptions> options)
        {
            _sitemapEntries = sitemapEntries;
            _options = options.Value;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // Use route value provided by SitemapTransformer template.
            var path = values["sitemap"] as string;

            if (!String.IsNullOrEmpty(path))
            {
                (var found, var sitemapId) = await _sitemapEntries.TryGetSitemapIdByPathAsync(path);

                if (found)
                {
                    var routeValues = new RouteValueDictionary(_options.GlobalRouteValues)
                    {
                        [_options.SitemapIdKey] = sitemapId
                    };

                    return routeValues;
                }
            }

            return null;
        }
    }
}
