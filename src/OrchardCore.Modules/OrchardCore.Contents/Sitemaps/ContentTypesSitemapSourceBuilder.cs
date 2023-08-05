using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Sitemaps.Aspects;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapSourceBuilder : SitemapSourceBuilderBase<ContentTypesSitemapSource>
    {
        private static readonly XNamespace _namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;
        private readonly IContentManager _contentManager;
        private readonly IContentItemsQueryProvider _contentItemsQueryProvider;
        private readonly IEnumerable<ISitemapContentItemExtendedMetadataProvider> _sitemapContentItemExtendedMetadataProviders;

        public ContentTypesSitemapSourceBuilder(
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator,
            IContentManager contentManager,
            IContentItemsQueryProvider contentItemsQueryProvider,
            IEnumerable<ISitemapContentItemExtendedMetadataProvider> sitemapContentItemExtendedMetadataProviders
            )
        {
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
            _contentManager = contentManager;
            _contentItemsQueryProvider = contentItemsQueryProvider;
            _sitemapContentItemExtendedMetadataProviders = sitemapContentItemExtendedMetadataProviders;
        }

        public override async Task BuildSourceAsync(ContentTypesSitemapSource source, SitemapBuilderContext context)
        {
            var queryContext = new ContentItemsQueryContext();
            await _contentItemsQueryProvider.GetContentItemsAsync(source, queryContext);

            foreach (var sciemp in _sitemapContentItemExtendedMetadataProviders)
            {
                context.Response.ResponseElement.Add(sciemp.GetExtendedAttribute);
            }

            foreach (var contentItem in queryContext.ContentItems)
            {
                var url = new XElement(_namespace + "url");

                if (await BuildUrlsetMetadataAsync(source, context, queryContext, contentItem, url))
                {
                    context.Response.ResponseElement.Add(url);
                }
            }
        }

        private async Task<bool> BuildUrlsetMetadataAsync(ContentTypesSitemapSource source, SitemapBuilderContext context, ContentItemsQueryContext queryContext, ContentItem contentItem, XElement url)
        {
            if (await BuildUrlAsync(context, contentItem, url))
            {
                if (await BuildExtendedMetadataAsync(context, queryContext, contentItem, url))
                {
                    PopulateLastMod(contentItem, url);
                    await PopulateChangeFrequencyPriority(source, contentItem, url);
                    return true;
                }

                return false;
            };

            return false;
        }

        private async Task<bool> BuildExtendedMetadataAsync(SitemapBuilderContext context, ContentItemsQueryContext queryContext, ContentItem contentItem, XElement url)
        {
            var suceeded = true;
            foreach (var sc in _sitemapContentItemExtendedMetadataProviders)
            {
                if (!await sc.ApplyExtendedMetadataAsync(context, queryContext, contentItem, url))
                {
                    suceeded = false;
                }
            }
            return suceeded;
        }

        private async Task<bool> BuildUrlAsync(SitemapBuilderContext context, ContentItem contentItem, XElement url)
        {
            var sitemapMetadataAspect = await _contentManager.PopulateAspectAsync<SitemapMetadataAspect>(contentItem);
            if (sitemapMetadataAspect.Exclude)
            {
                return false;
            }

            var locValue = await _routeableContentTypeCoordinator.GetRouteAsync(context, contentItem);

            var loc = new XElement(_namespace + "loc");
            loc.Add(locValue);
            url.Add(loc);
            return true;
        }

        private async Task PopulateChangeFrequencyPriority(ContentTypesSitemapSource source, ContentItem contentItem, XElement url)
        {
            var sitemapMetadataAspect = await _contentManager.PopulateAspectAsync<SitemapMetadataAspect>(contentItem);

            var changeFrequencyValue = sitemapMetadataAspect.ChangeFrequency;
            if (String.IsNullOrEmpty(changeFrequencyValue))
            {
                if (source.IndexAll)
                {
                    changeFrequencyValue = source.ChangeFrequency.ToString();
                }
                else if (source.LimitItems)
                {
                    changeFrequencyValue = source.LimitedContentType.ChangeFrequency.ToString();
                }
                else
                {
                    var sitemapEntry = source.ContentTypes
                        .FirstOrDefault(ct => String.Equals(ct.ContentTypeName, contentItem.ContentType));

                    changeFrequencyValue = sitemapEntry.ChangeFrequency.ToString();
                }
            }

            var priorityIntValue = sitemapMetadataAspect.Priority;
            if (!priorityIntValue.HasValue)
            {
                if (source.IndexAll)
                {
                    priorityIntValue = source.Priority;
                }
                else if (source.LimitItems)
                {
                    priorityIntValue = source.LimitedContentType.Priority;
                }
                else
                {
                    var sitemapEntry = source.ContentTypes
                        .FirstOrDefault(ct => String.Equals(ct.ContentTypeName, contentItem.ContentType));

                    priorityIntValue = sitemapEntry.Priority;
                }
            }

            var priorityValue = (priorityIntValue * 0.1f).Value.ToString(CultureInfo.InvariantCulture);

            var changeFreq = new XElement(_namespace + "changefreq");
            changeFreq.Add(changeFrequencyValue.ToLower());
            url.Add(changeFreq);

            var priority = new XElement(_namespace + "priority");
            priority.Add(priorityValue);
            url.Add(priority);
        }

        private static void PopulateLastMod(ContentItem contentItem, XElement url)
        {
            // Last modified is not required. Do not include if content item has no modified date.
            if (contentItem.ModifiedUtc.HasValue)
            {
                var lastMod = new XElement(_namespace + "lastmod");
                lastMod.Add(contentItem.ModifiedUtc.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz"));
                url.Add(lastMod);
            }
        }
    }
}
