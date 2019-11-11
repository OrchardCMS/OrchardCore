using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Sitemaps;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using OrchardCore.Sitemaps.Services;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.Sitemaps
{
    public class ContentTypesSitemapSourceBuilder : SitemapSourceBuilderBase<ContentTypesSitemapSource>
    {
        private static readonly XNamespace Namespace = "http://www.sitemaps.org/schemas/sitemap/0.9";

        private readonly IRouteableContentTypeCoordinator _routeableContentTypeCoordinator;
        private readonly ISitemapContentItemMetadataProvider _sitemapContentItemMetadataProvider;
        private readonly IContentItemsQueryProvider _contentItemsQueryProvider;
        private readonly SitemapsOptions _sitemapsOptions;
        private readonly IEnumerable<ISitemapContentItemValidationProvider> _sitemapContentItemValidationProviders;
        private readonly IEnumerable<ISitemapContentItemExtendedMetadataProvider> _sitemapContentItemExtendedMetadataProviders;
        private readonly ILogger _logger;

        public ContentTypesSitemapSourceBuilder(
            IRouteableContentTypeCoordinator routeableContentTypeCoordinator,
            ISitemapContentItemMetadataProvider sitemapContentItemMetadataProvider,
            IContentItemsQueryProvider contentItemsQueryProvider,
            IOptions<SitemapsOptions> options,
            IEnumerable<ISitemapContentItemValidationProvider> sitemapContentItemValidationProviders,
            IEnumerable<ISitemapContentItemExtendedMetadataProvider> sitemapContentItemExtendedMetadataProviders,
            ILogger<ContentTypesSitemapSourceBuilder> logger
            )
        {
            _routeableContentTypeCoordinator = routeableContentTypeCoordinator;
            _sitemapContentItemMetadataProvider = sitemapContentItemMetadataProvider;
            _contentItemsQueryProvider = contentItemsQueryProvider;
            _sitemapsOptions = options.Value;
            _sitemapContentItemValidationProviders = sitemapContentItemValidationProviders;
            _sitemapContentItemExtendedMetadataProviders = sitemapContentItemExtendedMetadataProviders;
            _logger = logger;
        }

        public override async Task BuildSourceAsync(ContentTypesSitemapSource source, SitemapBuilderContext context)
        {
            var queryContext = new ContentItemsQueryContext();
            await _contentItemsQueryProvider.GetContentItems(source, queryContext);

            foreach (var sciemp in _sitemapContentItemExtendedMetadataProviders)
            {
                context.Response.ResponseElement.Add(sciemp.GetExtendedAttribute);
            }

            foreach (var contentItem in queryContext.ContentItems)
            {
                var url = new XElement(Namespace + "url");

                if (await BuildUrlsetMetadataAsync(source, context, queryContext, contentItem, url))
                {
                    context.Response.ResponseElement.Add(url);
                }
            }
        }

        private async Task<bool> BuildUrlsetMetadataAsync(
            ContentTypesSitemapSource source,
            SitemapBuilderContext context,
            ContentItemsQueryContext queryContext,
            ContentItem contentItem,
            XElement url)
        {
            if (await BuildUrlAsync(context, contentItem, url))
            {
                if (await BuildExtendedMetadataAsync(context, queryContext, contentItem, url))
                {

                    BuildLastMod(contentItem, url);
                    BuildChangeFrequencyPriority(source, contentItem, url);
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

        private void BuildChangeFrequencyPriority(ContentTypesSitemapSource source, ContentItem contentItem, XElement url)
        {
            var changeFrequencyValue = _sitemapContentItemMetadataProvider.GetChangeFrequency(contentItem);
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

            var priorityIntValue = _sitemapContentItemMetadataProvider.GetPriority(contentItem);
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
