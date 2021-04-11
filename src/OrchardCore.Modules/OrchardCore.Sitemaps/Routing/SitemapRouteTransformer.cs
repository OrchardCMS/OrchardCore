using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Routing
{
    public class SitemapRouteTransformer : DynamicRouteValueTransformer
    {
        private readonly ISitemapManager _sitemapManager;
        private readonly SitemapsOptions _options;

        public SitemapRouteTransformer(ISitemapManager sitemapManager, IOptions<SitemapsOptions> options)
        {
            _sitemapManager = sitemapManager;
            _options = options.Value;
        }

        public override async ValueTask<RouteValueDictionary> TransformAsync(HttpContext httpContext, RouteValueDictionary values)
        {
            // Use route value provided by SitemapTransformer template.
            var path = values["sitemap"] as string;

            if (!String.IsNullOrEmpty(path))
            {
                (var found, var sitemapId) = await _sitemapManager.TryGetSitemapIdByPathAsync(path);

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
