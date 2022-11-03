using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Feeds;
using OrchardCore.Feeds.Models;
using OrchardCore.Lists.Indexes;
using YesSql;

namespace OrchardCore.Lists.Feeds
{
    public class ListFeedQuery : IFeedQueryProvider, IFeedQuery
    {
        public const int DefaultItemsCount = 20;

        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ListFeedQuery(IContentManager contentManager, ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public async Task<FeedQueryMatch> MatchAsync(FeedContext context)
        {
            var model = new ListFeedQueryViewModel();

            if (!await context.Updater.TryUpdateModelAsync(model) || model.ContentItemId == null)
            {
                return null;
            }

            var contentItem = await _contentManager.GetAsync(model.ContentItemId);
            var feedMetadata = await _contentManager.PopulateAspectAsync<FeedMetadata>(contentItem);

            if (feedMetadata.DisableRssFeed)
            {
                return null;
            }

            return new FeedQueryMatch { FeedQuery = this, Priority = -5 };
        }

        public async Task ExecuteAsync(FeedContext context)
        {
            var model = new ListFeedQueryViewModel();

            if (!await context.Updater.TryUpdateModelAsync(model) || model.ContentItemId == null)
            {
                return;
            }

            var contentItem = await _contentManager.GetAsync(model.ContentItemId);

            if (contentItem == null)
            {
                return;
            }

            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
            var bodyAspect = await _contentManager.PopulateAspectAsync<BodyAspect>(contentItem);
            var routes = contentItemMetadata.DisplayRouteValues;

            if (context.Format == "rss")
            {
                var link = new XElement("link");
                context.Response.Element.SetElementValue("title", contentItem.DisplayText);
                context.Response.Element.Add(link);

                context.Response.Element.Add(new XElement("description", new XCData(bodyAspect.Body?.ToString() ?? String.Empty)));

                context.Response.Contextualize(contextualize =>
                {
                    var request = contextualize.Url.ActionContext.HttpContext.Request;
                    var url = contextualize.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);

                    link.Add(url);
                });
            }
            else
            {
                context.Builder.AddProperty(context, null, "title", contentItem.DisplayText);
                context.Builder.AddProperty(context, null, new XElement("description", new XCData(bodyAspect.Body?.ToString() ?? String.Empty)));

                context.Response.Contextualize(contextualize =>
                {
                    var request = contextualize.Url.ActionContext.HttpContext.Request;
                    var url = contextualize.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);

                    context.Builder.AddProperty(context, null, "link", url);
                });
            }

            int itemsCount = contentItem.Content.ListPart?.FeedItemsCount ?? DefaultItemsCount;

            IEnumerable<ContentItem> items;

            items = itemsCount == 0
                ? Enumerable.Empty<ContentItem>()
                : await _session.Query<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == contentItem.ContentItemId)
                    .With<ContentItemIndex>(x => x.Published)
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(itemsCount)
                    .ListAsync();

            foreach (var item in items)
            {
                context.Builder.AddItem(context, item);
            }
        }
    }
}
