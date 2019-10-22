using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapBuilder : SitemapBuilderBase<ContentTypesSitemap>
    {
        private static readonly XNamespace Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private readonly ISession _session;
        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;
        private readonly ISitemapContentItemMetadataProvider _sitemapContentItemMetadataProvider;
        private readonly IEnumerable<ISitemapContentItemValidationProvider> _sitemapContentItemValidationProviders;
        private readonly IEnumerable<ISitemapContentItemExtendedMetadataProvider> _sitemapContentItemExtendedMetadataProviders;
        private readonly ILogger _logger;

        public ContentTypesSitemapBuilder(
            ISession session,
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator,
            ISitemapContentItemMetadataProvider sitemapContentItemMetadataProvider,
            IEnumerable<ISitemapContentItemValidationProvider> sitemapContentItemValidationProviders,
            IEnumerable<ISitemapContentItemExtendedMetadataProvider> sitemapContentItemExtendedMetadataProviders,
            ILogger<ContentTypesSitemapBuilder> logger
            )
        {
            _session = session;
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
            _sitemapContentItemMetadataProvider = sitemapContentItemMetadataProvider;
            _sitemapContentItemValidationProviders = sitemapContentItemValidationProviders;
            _sitemapContentItemExtendedMetadataProviders = sitemapContentItemExtendedMetadataProviders;
            _logger = logger;
        }

        public override async Task<XDocument> BuildSitemapsAsync(ContentTypesSitemap sitemap, SitemapBuilderContext context)
        {
            var contentItems = await GetContentItemsToBuildAsync(sitemap);

            var root = new XElement(Namespace + "urlset",
                new XAttribute("xmlns", Namespace));

            foreach (var sciemp in _sitemapContentItemExtendedMetadataProviders)
            {
                root.Add(sciemp.GetExtendedAttribute);
            }

            foreach (var contentItem in contentItems)
            {
                var url = new XElement(Namespace + "url");

                if (await BuildUrlsetMetadataAsync(sitemap, context, contentItems, contentItem, url))
                {
                    root.Add(url);
                }
            }
            var document = new XDocument(root);
            return new XDocument(document);
        }

        public override async Task<DateTime?> GetLastModifiedDateAsync(ContentTypesSitemap sitemap, SitemapBuilderContext context)
        {
            ContentItem mostRecentModifiedContentItem;

            if (sitemap.IndexAll)
            {
                var typesToIndex = _routeableContentTypeCoordinator
                    .ListRoutableTypeDefinitions()
                    .Select(ctd => ctd.Name);

                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(typesToIndex))
                    .OrderByDescending(x => x.ModifiedUtc);

                mostRecentModifiedContentItem = await query.FirstOrDefaultAsync();
            }
            else
            {
                var typesToIndex = _routeableContentTypeCoordinator
                    .ListRoutableTypeDefinitions()
                    .Where(ctd => sitemap.ContentTypes.Any(s => ctd.Name == s.ContentTypeName))
                    .Select(ctd => ctd.Name);

                // This is an estimate, so doesn't take into account Take/Skip values.
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(typesToIndex))
                    .OrderByDescending(x => x.ModifiedUtc);

                mostRecentModifiedContentItem = await query.FirstOrDefaultAsync();
            }
            return mostRecentModifiedContentItem.ModifiedUtc;
        }

        private async Task<bool> BuildUrlsetMetadataAsync(
            ContentTypesSitemap sitemap,
            SitemapBuilderContext context,
            IEnumerable<ContentItem> contentItems,
            ContentItem contentItem,
            XElement url)
        {
            if (await BuildUrlAsync(context, contentItem, url))
            {
                // TODO Build any extra values.
                // This will need the list to strip down localization sets.
                await BuildExtendedMetadataAsync(context, contentItems, contentItem, url);
                BuildLastMod(contentItem, url);
                BuildChangeFrequencyPriority(sitemap, contentItem, url);
                return true;
            };

            return false;
        }

        private async Task BuildExtendedMetadataAsync(SitemapBuilderContext context, IEnumerable<ContentItem> contentItems, ContentItem contentItem, XElement url)
        {
            foreach (var sc in _sitemapContentItemExtendedMetadataProviders)
            {
                await sc.ApplyExtendedMetadataAsync(context, contentItems, contentItem, url);
            }
        }

        private async Task<IEnumerable<ContentItem>> GetContentItemsToBuildAsync(ContentTypesSitemap sitemap)
        {
            var contentItems = new List<ContentItem>();

            var routeableContentTypeDefinitions = _routeableContentTypeCoordinator.ListRoutableTypeDefinitions();

            if (sitemap.IndexAll)
            {
                var rctdNames = routeableContentTypeDefinitions.Select(rctd => rctd.Name);

                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(rctdNames))
                    .OrderByDescending(x => x.CreatedUtc)
                    .ListAsync();

                contentItems = queryResults.ToList();

                if (contentItems.Count > 50000)
                {
                    _logger.LogError("Sitemap {Name} content item count is over 50,000", sitemap.Name);
                }
            }
            else
            {
                var validTypesToIndex = sitemap.ContentTypes
                    .Where(t => routeableContentTypeDefinitions.Any(x => x.Name == t.ContentTypeName));

                // Allow selection of multiple content types, but recommendation when
                // splitting a sitemap is to restrict the sitemap to a single content item.
                // TODO document this.

                // Process content types that have a skip/take value.
                foreach (var entry in validTypesToIndex.Where(x => !x.TakeAll))
                {
                    var takeSomeQueryResults = await _session.Query<ContentItem>()
                        .With<ContentItemIndex>(x => x.ContentType == entry.ContentTypeName && x.Published)
                        .OrderByDescending(x => x.CreatedUtc)
                        .Skip(entry.Skip)
                        .Take(entry.Take)
                        .ListAsync();

                    contentItems.AddRange(takeSomeQueryResults);
                }

                // Process content types without skip/take value.
                var typesToIndex = routeableContentTypeDefinitions
                    .Where(ctd => sitemap.ContentTypes.Where(x => x.TakeAll).Any(s => ctd.Name == s.ContentTypeName))
                    .Select(x => x.Name);

                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                    .OrderByDescending(x => x.CreatedUtc)
                    .ListAsync();

                contentItems.AddRange(queryResults);
            }

            return contentItems;
        }

        private async Task<bool> BuildUrlAsync(SitemapBuilderContext context, ContentItem contentItem, XElement url)
        {
            var isValid = false;
            foreach (var validationProvider in _sitemapContentItemValidationProviders)
            {
                isValid = await validationProvider.ValidateContentItem(contentItem);
            }

            if (!isValid)
            {
                return false;
            }

            var locValue = await _routeableContentTypeCoordinator.GetRouteAsync(context, contentItem);

            var loc = new XElement(Namespace + "loc");
            loc.Add(locValue);
            url.Add(loc);
            return true;
        }

        private void BuildChangeFrequencyPriority(ContentTypesSitemap sitemap, ContentItem contentItem, XElement url)
        {
            var changeFrequencyValue = _sitemapContentItemMetadataProvider.GetChangeFrequency(contentItem);
            if (String.IsNullOrEmpty(changeFrequencyValue))
            {
                if (sitemap.IndexAll)
                {
                    changeFrequencyValue = sitemap.ChangeFrequency.ToString();
                }
                else
                {
                    var sitemapEntry = sitemap.ContentTypes
                        .FirstOrDefault(x => x.ContentTypeName == contentItem.ContentType);

                    changeFrequencyValue = sitemapEntry.ChangeFrequency.ToString();
                }
            }

            var priorityIntValue = _sitemapContentItemMetadataProvider.GetPriority(contentItem);
            if (!priorityIntValue.HasValue)
            {
                if (sitemap.IndexAll)
                {
                    priorityIntValue = sitemap.Priority;
                }
                else
                {
                    var sitemapEntry = sitemap.ContentTypes
                        .FirstOrDefault(x => x.ContentTypeName == contentItem.ContentType);

                    priorityIntValue = sitemapEntry.Priority;
                }
            }

            var priorityValue = (priorityIntValue * 0.1f).Value.ToString(CultureInfo.InvariantCulture);

            var changeFreq = new XElement(Namespace + "changefreq");
            changeFreq.Add(changeFrequencyValue.ToLower());
            url.Add(changeFreq);

            var priority = new XElement(Namespace + "priority");
            priority.Add(priorityValue);
            url.Add(priority);
        }

        private void BuildLastMod(ContentItem contentItem, XElement url)
        {
            // Last modified is not required. Do not include if content item has no modified date.
            if (contentItem.ModifiedUtc.HasValue)
            {
                var lastMod = new XElement(Namespace + "lastmod");
                lastMod.Add(contentItem.ModifiedUtc.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz"));
                url.Add(lastMod);
            }
        }
    }
}
