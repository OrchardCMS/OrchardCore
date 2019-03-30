using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Models;
using YesSql;

namespace OrchardCore.Contents.SitemapNodes
{
    public class ContentTypesSitemapBuilder : UrlsetSitemapNodeBuilderBase<ContentTypesSitemapNode>
    {
        public ContentTypesSitemapBuilder(
            ILogger<ContentTypesSitemapBuilder> logger,
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISiteService siteService)
            : base(logger,
                  session,
                  contentDefinitionManager,
                  contentManager,
                  siteService)
        { }



        protected async override Task<bool> BuildUrlsetMetadata(ContentTypesSitemapNode sitemapNode, SitemapBuilderContext context, RouteValueDictionary homeRoute, ContentItem contentItem, XElement url)
        {
            if (await base.BuildUrlsetMetadata(sitemapNode, context, homeRoute, contentItem, url))
            {
                BuildLastMod(contentItem, url);
                BuildChangeFrequencyPriority(sitemapNode, contentItem, url);
                return true;
            };
            return false;
        }
        private void BuildChangeFrequencyPriority(ContentTypesSitemapNode sitemapNode, ContentItem contentItem, XElement url)
        {
            string changeFrequencyValue = null;
            string priorityValue = null;
            if (sitemapNode.IndexAll)
            {
                changeFrequencyValue = sitemapNode.ChangeFrequency.ToString();
                priorityValue = sitemapNode.Priority.ToString();
                if (contentItem.Has<SitemapPart>())
                {
                    var part = contentItem.As<SitemapPart>();
                    if (part.OverrideSettings)
                    {
                        changeFrequencyValue = part.ChangeFrequency.ToString();
                        priorityValue = part.Priority.ToString();
                    }
                }
            }
            else
            {
                var sitemapEntry = sitemapNode.ContentTypes.FirstOrDefault(x => String.Equals(x.ContentTypeId, contentItem.ContentType, StringComparison.OrdinalIgnoreCase));
                changeFrequencyValue = sitemapEntry.ChangeFrequency.ToString();
                priorityValue = sitemapEntry.IndexPriority.ToString();
                if (contentItem.Has<SitemapPart>())
                {
                    var part = contentItem.As<SitemapPart>();
                    if (part.OverrideSettings)
                    {
                        changeFrequencyValue = part.ChangeFrequency.ToString();
                        priorityValue = part.Priority.ToString();
                    }
                }
            }

            var changeFreq = new XElement(GetNamespace() + "changefreq");
            changeFreq.Add(changeFrequencyValue.ToLower());
            url.Add(changeFreq);

            var priority = new XElement(GetNamespace() + "priority");
            priority.Add(priorityValue);
            url.Add(priority);
        }

        private void BuildLastMod(ContentItem contentItem, XElement url)
        {
            var lastMod = new XElement(GetNamespace() + "lastmod");
            lastMod.Add(contentItem.ModifiedUtc.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:sszzz"));
            url.Add(lastMod);
        }


    }
}
