using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Sitemaps.Sitemaps
{
    public class SitemapIndexBuilder : SitemapBuilderBase<SitemapIndex>
    {
        public override async Task<XDocument> BuildSitemapsAsync(SitemapIndex sitemapIndex, SitemapBuilderContext context)
        {
            XNamespace defaultNamespace = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var root = new XElement(defaultNamespace + "sitemapindex");

            var containedSitemaps = (await context.SitemapManager.ListSitemapsAsync())
                .Where(s => s.Enabled && sitemapIndex.ContainedSitemapIds.Any(id => id == s.Id));

            foreach (var sitemap in containedSitemaps)
            {
                var xmlSitemap = new XElement(defaultNamespace + "sitemap");
                var loc = new XElement(defaultNamespace + "loc");
                loc.Add(context.HostPrefix + context.UrlHelper.ActionContext.HttpContext.Request.PathBase + "/" + sitemap.Path);
                xmlSitemap.Add(loc);

                var lastModDate = await context.SitemapManager.GetSitemapLastModifiedDateAsync(sitemap, context);
                var lastMod = new XElement(defaultNamespace + "lastmod");
                lastMod.Add(lastModDate.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz"));
                xmlSitemap.Add(lastMod);

                root.Add(xmlSitemap);
            }
            var document = new XDocument(root);
            return new XDocument(document);
        }
    }
}
