using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Aspects;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.ContentLocalization.Sitemaps
{
    public class SitemapUrlHrefLangExtendedMetadataProvider : ISitemapContentItemExtendedMetadataProvider
    {
        private static readonly XNamespace _extendedNamespace = "http://www.w3.org/1999/xhtml";
        private static readonly XAttribute _extendedAttribute = new(XNamespace.Xmlns + "xhtml", _extendedNamespace);

        private readonly IContentManager _contentManager;
        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;

        public SitemapUrlHrefLangExtendedMetadataProvider(
            IContentManager contentManager,
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator
            )
        {
            _contentManager = contentManager;
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
        }

        public XAttribute GetExtendedAttribute => _extendedAttribute;

        public async Task<bool> ApplyExtendedMetadataAsync(
            SitemapBuilderContext context,
            ContentItemsQueryContext queryContext,
            ContentItem contentItem,
            XElement url)
        {
            var part = contentItem.As<LocalizationPart>();
            if (part == null)
            {
                return true;
            }

            var localizedContentParts = queryContext.ReferenceContentItems
                .Select(ci => ci.As<LocalizationPart>())
                .Where(cp => cp.LocalizationSet == part.LocalizationSet);

            foreach (var localizedPart in localizedContentParts)
            {
                var sitemapMetadataAspect = await _contentManager.PopulateAspectAsync<SitemapMetadataAspect>(localizedPart.ContentItem);
                if (sitemapMetadataAspect.Exclude)
                {
                    continue;
                }

                var hrefValue = await _routeableContentTypeCoordinator.GetRouteAsync(context, localizedPart.ContentItem);

                var linkNode = new XElement(_extendedNamespace + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", localizedPart.Culture),
                    new XAttribute("href", hrefValue));

                url.Add(linkNode);
            }

            return true;
        }
    }
}
