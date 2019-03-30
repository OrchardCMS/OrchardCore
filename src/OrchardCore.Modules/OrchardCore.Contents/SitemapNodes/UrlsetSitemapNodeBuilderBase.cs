using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using YesSql;

namespace OrchardCore.Contents.SitemapNodes
{
    public abstract class UrlsetSitemapNodeBuilderBase<TSitemapNode> : SitemapNodeBuilderBase<TSitemapNode> where TSitemapNode : ContentTypesSitemapNode
    {
        protected readonly ISession _session;
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly IContentManager _contentManager;
        protected readonly ISiteService _siteService;
        public UrlsetSitemapNodeBuilderBase(
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISiteService siteService)
        {
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _siteService = siteService;
        }

        public override async Task BuildNodeAsync(TSitemapNode sitemapNode, SitemapBuilderContext context)
        {
            var homeRoute = await GetHomeRoute();
            var contentItems = await GetContentItems(sitemapNode);

            var root = new XElement(GetNamespace() + "urlset");

            foreach (var contentItem in contentItems)
            {
                var url = new XElement(GetNamespace() + "url");

                if (await BuildUrlsetMetadata(sitemapNode, context, homeRoute, contentItem, url))
                {
                    root.Add(url);
                }

            }
            var document = new XDocument(root);
            context.Result = new XDocument(document);
        }

        protected virtual async Task<bool> BuildUrlsetMetadata(ContentTypesSitemapNode sitemapNode, SitemapBuilderContext context, RouteValueDictionary homeRoute, ContentItem contentItem, XElement url)
        {
            return await BuildUrl(context, contentItem, homeRoute, url);
        }

        protected virtual async Task<IEnumerable<ContentItem>> GetContentItems(TSitemapNode sitemapNode)
        {
            IEnumerable<ContentItem> contentItems;
            if (sitemapNode.IndexAll)
            {
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published)
                    .OrderBy(x => x.ModifiedUtc);

                contentItems = await query.ListAsync();
            }
            else
            {
                var typesToList = _contentDefinitionManager.ListTypeDefinitions()
                    .Where(ctd => sitemapNode.ContentTypes.ToList()
                    .Any(s => String.Equals(ctd.Name, s.ContentTypeId, StringComparison.OrdinalIgnoreCase)))
                    .Select(x => x.Name);

                var contentTypes = sitemapNode.ContentTypes.Select(x => x.ContentTypeId);
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => typesToList.Any(n => n == x.ContentType) && x.Published)
                    .OrderBy(x => x.ModifiedUtc);
                contentItems = await query.ListAsync();
            }

            return contentItems;
        }

        protected virtual async Task<RouteValueDictionary> GetHomeRoute()
        {
            return (await _siteService.GetSiteSettingsAsync()).HomeRoute;
        }

        protected virtual async Task<bool> BuildUrl(SitemapBuilderContext context, ContentItem contentItem, RouteValueDictionary homeRoute, XElement url)
        {
            //if we have no autoroute part we shouldn't include in index
            //this stops contained contentitems (and items like the footer) getting built.
            //there maybe a better way of determining this. 
            if (contentItem.Content.AutoroutePart == null)
            {
                return false;
            }
            var sitemapPart = contentItem.As<SitemapPart>();
            if (sitemapPart != null && sitemapPart.Exclude)
            {
                return false; // sitemapPart not required, but to exclude or override defaults add it to the ContentItem
            }
            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            var routes = contentItemMetadata.DisplayRouteValues;

            String locValue = null;
            var isHomePage = homeRoute.All(x => x.Value.ToString() == routes[x.Key].ToString());
            if (isHomePage)
            {
                locValue = context.Url.GetBaseUrl();
            }
            var request = context.Url.ActionContext.HttpContext.Request;
            locValue = context.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);

            var loc = new XElement(GetNamespace() + "loc");
            loc.Add(locValue);
            url.Add(loc);
            return true;

        }
        protected virtual XNamespace GetNamespace()
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            return xmlns;
        }
    }
}
