using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Builders
{
    public class SitemapIndexTypeBuilder : SitemapTypeBuilderBase<SitemapIndex>
    {
        private static readonly XNamespace _namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
        private static readonly XNamespace _schemaInstance = "http://www.w3.org/2001/XMLSchema-instance";
        private static readonly XNamespace _schemaLocation = "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd";

        private readonly ISitemapManager _sitemapManager;
        private readonly ISitemapModifiedDateProvider _sitemapModifiedDateProvider;
        private readonly SitemapsOptions _sitemapsOptions;

        public SitemapIndexTypeBuilder(
            ISitemapManager sitemapManager,
            ISitemapModifiedDateProvider sitemapModifiedDateProvider,
            IOptions<SitemapsOptions> options
            )
        {
            _sitemapManager = sitemapManager;
            _sitemapModifiedDateProvider = sitemapModifiedDateProvider;
            _sitemapsOptions = options.Value;
        }

        public override async Task BuildSitemapTypeAsync(SitemapIndex sitemap, SitemapBuilderContext context)
        {
            context.Response = new SitemapResponse
            {
                ResponseElement = new XElement(_namespace + "sitemapindex",
                    new XAttribute(XNamespace.Xmlns + "xsi", _schemaInstance),
                    new XAttribute(_schemaInstance + "schemaLocation", _schemaLocation))
            };

            var indexSource = sitemap.SitemapSources.FirstOrDefault() as SitemapIndexSource;

            if (indexSource == null)
            {
                return;
            }

            var containedSitemaps = (await _sitemapManager.GetSitemapsAsync())
                .Where(s => s.Enabled && indexSource.ContainedSitemapIds.Any(id => id == s.SitemapId));

            foreach (var containedSitemap in containedSitemaps)
            {
                var xmlSitemap = new XElement(_namespace + "sitemap");
                var loc = new XElement(_namespace + "loc");

                var routeValues = new RouteValueDictionary(_sitemapsOptions.GlobalRouteValues)
                {
                    [_sitemapsOptions.SitemapIdKey] = containedSitemap.SitemapId
                };

                loc.Add(context.HostPrefix + context.UrlHelper.Action(routeValues["Action"].ToString(), routeValues));
                xmlSitemap.Add(loc);

                var lastModDate = await _sitemapModifiedDateProvider.GetLastModifiedDateAsync(containedSitemap);
                if (lastModDate.HasValue)
                {
                    var lastMod = new XElement(_namespace + "lastmod");
                    lastMod.Add(lastModDate.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
                    xmlSitemap.Add(lastMod);
                }

                context.Response.ResponseElement.Add(xmlSitemap);
            }
        }
    }
}
