using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
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
        private readonly IEnumerable<IRouteableContentTypeDefinitionProvider> _routeableContentTypeDefinitionProviders;
        private readonly IContentManager _contentManager;
        private readonly ILogger _logger;

        public ContentTypesSitemapBuilder(
            ISession session,
            IEnumerable<IRouteableContentTypeDefinitionProvider> routeableContentTypeDefinitionProviders,
            IContentManager contentManager,
            ILogger<ContentTypesSitemapBuilder> logger
            )
        {
            _session = session;
            _routeableContentTypeDefinitionProviders = routeableContentTypeDefinitionProviders;
            _contentManager = contentManager;
            _logger = logger;
        }

        public override async Task<XDocument> BuildSitemapsAsync(ContentTypesSitemap sitemap, SitemapBuilderContext context)
        {
            var contentItems = await GetContentItemsToBuildAsync(sitemap);

            var root = new XElement(Namespace + "urlset");

            foreach (var contentItem in contentItems)
            {
                var url = new XElement(Namespace + "url");

                if (await BuildUrlsetMetadataAsync(sitemap, context, contentItem, url))
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
                var typesToIndex = _routeableContentTypeDefinitionProviders.SelectMany(ctd => ctd.ListRoutableTypeDefinitions());

                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(typesToIndex))
                    .OrderByDescending(x => x.ModifiedUtc);

                mostRecentModifiedContentItem = await query.FirstOrDefaultAsync();
            }
            else
            {
                var typesToIndex = _routeableContentTypeDefinitionProviders.SelectMany(ctd => ctd.ListRoutableTypeDefinitions())
                    .Where(ctd => sitemap.ContentTypes.Any(s => ctd.Name == s.ContentTypeName))
                    .Select(x => x.Name);

                // This is an estimate, so doesn't take into account Take/Skip values.
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(typesToIndex))
                    .OrderByDescending(x => x.ModifiedUtc);

                mostRecentModifiedContentItem = await query.FirstOrDefaultAsync();
            }
            return mostRecentModifiedContentItem.ModifiedUtc;
        }

        private async Task<bool> BuildUrlsetMetadataAsync(ContentTypesSitemap sitemap,
            SitemapBuilderContext context,
            ContentItem contentItem,
            XElement url)
        {
            if (await BuildUrlAsync(context, contentItem, url))
            {
                BuildLastMod(contentItem, url);
                BuildChangeFrequencyPriority(sitemap, contentItem, url);
                return true;
            };

            return false;
        }

        private async Task<IEnumerable<ContentItem>> GetContentItemsToBuildAsync(ContentTypesSitemap sitemap)
        {
            var contentItems = new List<ContentItem>();

            var routeableContentTypeDefinitions = _routeableContentTypeDefinitionProviders
                .SelectMany(ctd => ctd.ListRoutableTypeDefinitions());

            if (sitemap.IndexAll)
            {
                var rctdNames = routeableContentTypeDefinitions.Select(rctd => rctd.Name);

                var queryResults = await _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published && x.ContentType.IsIn(rctdNames))
                    .OrderByDescending(x => x.ModifiedUtc)
                    .ListAsync();

                contentItems = queryResults.ToList();

                if (contentItems.Count() > 50000)
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
                        .OrderByDescending(x => x.ModifiedUtc)
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
                    .OrderByDescending(x => x.ModifiedUtc)
                    .ListAsync();

                contentItems.AddRange(queryResults);
            }

            return contentItems;
        }

        private async Task<bool> BuildUrlAsync(SitemapBuilderContext context, ContentItem contentItem, XElement url)
        {
            // TODO resolve this dependance on SitemapPart through
            // a Sitemap Content Item Validator

            // SitemapPart is optional, but to exclude or override defaults add it to the ContentItem
            var sitemapPart = contentItem.As<SitemapPart>();
            if (sitemapPart != null && sitemapPart.OverrideSitemapConfig && sitemapPart.Exclude)
            {
                return false;
            }

            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            var routes = contentItemMetadata.DisplayRouteValues;

            // UrlHelper.Action includes BasePath automatically if present.
            // If content item is assigned as home route, Urlhelper resolves as site root.
            var locValue = context.HostPrefix + context.UrlHelper.Action(routes["Action"].ToString(), routes);

            var loc = new XElement(Namespace + "loc");
            loc.Add(locValue);
            url.Add(loc);
            return true;
        }

        private void BuildChangeFrequencyPriority(ContentTypesSitemap sitemap, ContentItem contentItem, XElement url)
        {
            string changeFrequencyValue = null;
            string priorityValue = null;
            if (sitemap.IndexAll)
            {
                changeFrequencyValue = sitemap.ChangeFrequency.ToString();
                priorityValue = sitemap.Priority.ToString();
            }
            else
            {
                var sitemapEntry = sitemap.ContentTypes.FirstOrDefault(x => x.ContentTypeName == contentItem.ContentType);
                changeFrequencyValue = sitemapEntry.ChangeFrequency.ToString();
                priorityValue = sitemapEntry.Priority.ToString();
            }

            //TODO move this to a validation provider and move sitemap part back to sitmapes.
            if (contentItem.Has<SitemapPart>())
            {
                var part = contentItem.As<SitemapPart>();
                if (part.OverrideSitemapConfig)
                {
                    changeFrequencyValue = part.ChangeFrequency.ToString();
                    priorityValue = part.Priority.ToString();
                }
            }

            var changeFreq = new XElement(Namespace + "changefreq");
            changeFreq.Add(changeFrequencyValue.ToLower());
            url.Add(changeFreq);

            var priority = new XElement(Namespace + "priority");
            priority.Add(priorityValue);
            url.Add(priority);
        }

        private void BuildLastMod(ContentItem contentItem, XElement url)
        {
            var lastMod = new XElement(Namespace + "lastmod");
            lastMod.Add(contentItem.ModifiedUtc.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz"));
            url.Add(lastMod);
        }
    }
}
