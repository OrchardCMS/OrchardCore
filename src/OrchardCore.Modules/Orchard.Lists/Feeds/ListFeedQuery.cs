using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Models;
using Orchard.ContentManagement.Records;
using Orchard.Feeds;
using Orchard.Feeds.Models;
using Orchard.Lists.Indexes;
using YesSql;

namespace Orchard.Lists.Feeds
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

        public FeedQueryMatch Match(FeedContext context)
        {
            var model = new ListFeedQueryViewModel();

            if(!context.Updater.TryUpdateModelAsync(model).GetAwaiter().GetResult() || model.ContentItemId == null)
            {
                return null;
            }

            return new FeedQueryMatch { FeedQuery = this, Priority = -5 };
        }

        public async Task ExecuteAsync(FeedContext context)
        {
            var model = new ListFeedQueryViewModel();

            if (!context.Updater.TryUpdateModelAsync(model).GetAwaiter().GetResult() || model.ContentItemId == null)
            {
                return;
            }

            var contentItem = _contentManager.GetAsync(model.ContentItemId).GetAwaiter().GetResult();

            if (contentItem == null)
            {
                return;
            }

            var contentItemMetadata = _contentManager.PopulateAspect<ContentItemMetadata>(contentItem);
            var bodyAspect = _contentManager.PopulateAspect<BodyAspect>(contentItem);
            var routes = contentItemMetadata.DisplayRouteValues;

            if (context.Format == "rss")
            {
                var link = new XElement("link");
                context.Response.Element.SetElementValue("title", contentItemMetadata.DisplayText);
                context.Response.Element.Add(link);

                if (bodyAspect.Body != null)
                {
                    context.Response.Element.SetElementValue("description", bodyAspect.Body.ToString());
                }

                context.Response.Contextualize(contextualize =>
                {
                    var request = contextualize.Url.ActionContext.HttpContext.Request;
                    var url = contextualize.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);

                    link.Add(url);
                });
            }
            else
            {
                context.Builder.AddProperty(context, null, "title", contentItemMetadata.DisplayText);

                if (bodyAspect.Body != null)
                {
                    context.Builder.AddProperty(context, null, "description", bodyAspect.Body.ToString());
                }

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
                : await _session.QueryAsync<ContentItem>()
                    .With<ContainedPartIndex>(x => x.ListContentItemId == contentItem.ContentItemId)
                    .With<ContentItemIndex>(x => x.Published)
                    .OrderByDescending(x => x.CreatedUtc)
                    .Take(itemsCount)
                    .List();

            foreach (var item in items)
            {
                context.Builder.AddItem(context, item);
            }
        }
    }
}