using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.ContentLocalization.Sitemaps
{
    public class SitemapUrlHrefLangExtendedMetadataProvider : ISitemapContentItemExtendedMetadataProvider
    {
        private static readonly XNamespace ExtendedNamespace = "http://www.w3.org/TR/xhtml11/xhtml11_schema.html";
        private static readonly XAttribute ExtendedAttribute = new XAttribute(XNamespace.Xmlns + "xhtml", ExtendedNamespace);

        private readonly ISitemapPartContentItemValidationProvider _sitemapPartContentItemValidationProvider;
        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;

        public SitemapUrlHrefLangExtendedMetadataProvider(
            ISitemapPartContentItemValidationProvider sitemapPartContentItemValidationProvider,
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator
            )
        {
            _sitemapPartContentItemValidationProvider = sitemapPartContentItemValidationProvider;
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
        }

        public XAttribute GetExtendedAttribute => ExtendedAttribute;

        public async Task ApplyExtendedMetadataAsync(SitemapBuilderContext context,
            IEnumerable<ContentItem> contentItems,
            ContentItem contentItem,
            XElement url)
        {
            var part = contentItem.As<LocalizationPart>();
            if (part == null)
            {
                return;
            }

            //TODO Optimize. Consider an ItemContext perhaps.
            var allLocalizedContentParts = contentItems
                .Where(ci => ci.As<LocalizationPart>() != null)
                .Select(ci => ci.As<LocalizationPart>());

            var localizedContentParts = allLocalizedContentParts
                .Where(cp => cp.LocalizationSet == part.LocalizationSet);

            foreach (var localizedPart in localizedContentParts)
            {
                if (!await _sitemapPartContentItemValidationProvider.ValidateContentItem(localizedPart.ContentItem))
                {
                    continue;
                }

                var hrefValue = await _routeableContentTypeCoordinator.GetRouteAsync(context, localizedPart.ContentItem);

                var linkNode = new XElement(ExtendedNamespace + "link",
                    new XAttribute("rel", "alternate"),
                    new XAttribute("hreflang", localizedPart.Culture),
                    new XAttribute("href", hrefValue));

                url.Add(linkNode);
            }
        }
    }
}
