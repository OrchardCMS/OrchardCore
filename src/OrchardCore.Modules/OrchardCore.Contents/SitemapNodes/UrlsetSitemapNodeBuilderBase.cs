using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Sitemaps.Builders;
using OrchardCore.Sitemaps.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.Contents.SitemapNodes
{
    public abstract class UrlsetSitemapNodeBuilderBase<TSitemapNode> : SitemapNodeBuilderBase<TSitemapNode> where TSitemapNode : ContentTypesSitemapNode
    {
        protected readonly ISession _session;
        protected readonly IContentDefinitionManager _contentDefinitionManager;
        protected readonly IContentManager _contentManager;
        protected readonly ISiteService _siteService;
        public UrlsetSitemapNodeBuilderBase(
            ILogger logger,
            ISession session,
            IContentDefinitionManager contentDefinitionManager,
            IContentManager contentManager,
            ISiteService siteService)
        {
            Logger = logger;
            _session = session;
            _contentDefinitionManager = contentDefinitionManager;
            _contentManager = contentManager;
            _siteService = siteService;
        }

        public ILogger Logger { get; }
        public override async Task<XDocument> BuildNodeAsync(TSitemapNode sitemapNode, SitemapBuilderContext context)
        {
            //this does not need to recurse sitemapNode.ChildNodes. urlsets do not support children. they are end of level
            var homeRoute = await GetHomeRoute();
            var contentItems = await GetContentItemsToBuild(sitemapNode);

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
            return new XDocument(document);
        }

        public override async Task<DateTime?> ProvideNodeLastModifiedDateAsync(TSitemapNode sitemapNode, SitemapBuilderContext context)
        {
            ContentItem mostRecentModifiedContentItem;
            if (sitemapNode.IndexAll)
            {
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published)
                    .OrderByDescending(x => x.ModifiedUtc);

                mostRecentModifiedContentItem = await query.FirstOrDefaultAsync();
            }
            else
            {
                var typesToIndex = _contentDefinitionManager.ListTypeDefinitions()
                    .Where(ctd => sitemapNode.ContentTypes.ToList()
                    .Any(s => ctd.Name == s.ContentTypeName))
                    .Select(x => x.Name);

                var contentTypes = sitemapNode.ContentTypes.Select(x => x.ContentTypeName);
                //this doesn't use the takeall/skip option. should be ok tho its just an estimate date
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                    .OrderByDescending(x => x.ModifiedUtc);
                mostRecentModifiedContentItem = await query.FirstOrDefaultAsync();
            }
            return mostRecentModifiedContentItem.ModifiedUtc;
        }

        protected virtual async Task<bool> BuildUrlsetMetadata(ContentTypesSitemapNode sitemapNode, SitemapBuilderContext context, RouteValueDictionary homeRoute, ContentItem contentItem, XElement url)
        {
            return await BuildUrl(context, contentItem, homeRoute, url);
        }

        protected virtual async Task<IEnumerable<ContentItem>> GetContentItemsToBuild(TSitemapNode sitemapNode)
        {
            List<ContentItem> contentItems = new List<ContentItem>();
            if (sitemapNode.IndexAll)
            {
                var query = _session.Query<ContentItem>()
                    .With<ContentItemIndex>(x => x.Published)
                    .OrderByDescending(x => x.ModifiedUtc);

                contentItems = (await query.ListAsync()).ToList();
                if (contentItems.Count() > 50000)
                {
                    Logger.LogError($"Sitemap {sitemapNode.Description} count is over 50,000");
                }
            }
            else
            {
                //we assume that if sitemap is split (e.g. !TakeAll) that it's likely there will be only one content type
                //in this particular sitemap, but we still loop the queries to .Skip & .Take - just in case,
                //rather than reducing with Linq after one big content item query 
                //TODO make a note in readme -> large content item recommendations
                if (sitemapNode.ContentTypes.Any(x => !x.TakeAll))
                {
                    List<Task<IEnumerable<ContentItem>>> tasks = new List<Task<IEnumerable<ContentItem>>>();
                    foreach(var takeSomeType in sitemapNode.ContentTypes.Where(x => !x.TakeAll))
                    {
                        var query = _session.Query<ContentItem>()
                            .With<ContentItemIndex>(x => x.ContentType == takeSomeType.ContentTypeName && x.Published)
                            .OrderByDescending(x => x.ModifiedUtc)
                            .Skip(takeSomeType.Skip)
                            .Take(takeSomeType.Take);
                        tasks.Add(query.ListAsync());
                    }
                    //do others
                    var typesToIndex = _contentDefinitionManager.ListTypeDefinitions()
                        .Where(ctd => sitemapNode.ContentTypes.Where(x => x.TakeAll).ToList().Any(s => ctd.Name == s.ContentTypeName))
                        .Select(x => x.Name);
                    var othersQuery = _session.Query<ContentItem>()
                        .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                        .OrderByDescending(x => x.ModifiedUtc);

                    tasks.Add(othersQuery.ListAsync());
                    await Task.WhenAll(tasks);
                    tasks.ForEach(x => contentItems.AddRange(x.Result));
                }
                else
                {
                    var typeDef = _contentDefinitionManager.ListTypeDefinitions().ToList();
                    var typesTo = _contentDefinitionManager.ListTypeDefinitions()
                       .Where(ctd => sitemapNode.ContentTypes.ToList().Any(s => ctd.Name == s.ContentTypeName))
                       .Select(t => t.Name)
                       .ToList();

                    var typesToIndex = _contentDefinitionManager.ListTypeDefinitions()
                       .Where(ctd => sitemapNode.ContentTypes.ToList().Any(s => ctd.Name == s.ContentTypeName))
                       .Select(x => x.Name)
                       .ToList();

                    var query = _session.Query<ContentItem>()
                        .With<ContentItemIndex>(x => x.ContentType.IsIn(typesToIndex) && x.Published)
                        .OrderByDescending(x => x.ModifiedUtc);

                    contentItems = (await query.ListAsync()).ToList();
                }
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
            if (sitemapPart != null && sitemapPart.OverrideSitemapSetConfig && sitemapPart.Exclude)
            {
                return false; // sitemapPart not required, but to exclude or override defaults add it to the ContentItem
            }
            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            var routes = contentItemMetadata.DisplayRouteValues;

            //hmmm what should happen here. should both be provided? site root and /route
            //if you're swapping homepages regularly maybe. but if you're not you don't want your /route to turn up in google and
            //take preference over your site /. (or even appear beside it in search results.
            //I always put my home route with a blank path - which explains some of the weirdness
            //I get trying to get a good url out of url.action on my home route. but the blog theme provides /blog
            String locValue = null;
            var isHomePage = homeRoute.All(x => x.Value.ToString() == routes[x.Key].ToString());
            if (isHomePage)
            {
                locValue = context.Url.GetBaseUrl();
            }
            else
            {
                var request = context.Url.ActionContext.HttpContext.Request;
                locValue = context.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);
            }
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
