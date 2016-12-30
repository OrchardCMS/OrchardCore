using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Models;
using Orchard.Feeds;
using Orchard.Feeds.Models;

namespace Orchard.Contents.Feeds.Builders
{
    public class CommonFeedItemBuilder : IFeedItemBuilder
    {
        private readonly IContentManager _contentManager;

        public CommonFeedItemBuilder(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void Populate(FeedContext context)
        {
            foreach (var feedItem in context.Response.Items.OfType<FeedItem<ContentItem>>())
            {
                var contentItem = feedItem.Item;
                var contentItemMetadata = _contentManager.PopulateAspect<ContentItemMetadata>(contentItem);
                var bodyAspect = _contentManager.PopulateAspect<BodyAspect>(contentItem);
                var routes = contentItemMetadata.DisplayRouteValues;

                // author is intentionally left empty as it could result in unwanted spam

                // add to known formats
                if (context.Format == "rss")
                {
                    var link = new XElement("link");
                    var guid = new XElement("guid", new XAttribute("isPermaLink", "true"));

                    context.Response.Contextualize(contextualize =>
                    {
                        var request = contextualize.Url.ActionContext.HttpContext.Request;
                        var url = contextualize.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);

                        link.Add(url);
                        guid.Add(url);
                    });

                    feedItem.Element.SetElementValue("title", contentItemMetadata.DisplayText);
                    feedItem.Element.Add(link);
                    feedItem.Element.SetElementValue("description", bodyAspect.Body.ToString());

                    if (contentItem.PublishedUtc != null)
                    {
                        // RFC833 
                        // The "R" or "r" standard format specifier represents a custom date and time format string that is defined by 
                        // the DateTimeFormatInfo.RFC1123Pattern property. The pattern reflects a defined standard, and the property  
                        // is read-only. Therefore, it is always the same, regardless of the culture used or the format provider supplied.  
                        // The custom format string is "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'". When this standard format specifier is used,  
                        // the formatting or parsing operation always uses the invariant culture. 
                        feedItem.Element.SetElementValue("pubDate", contentItem.PublishedUtc.Value.ToString("r"));
                    }

                    feedItem.Element.Add(guid);
                }
                else
                {
                    context.Response.Contextualize(contextualize => 
                    {
                        var request = contextualize.Url.ActionContext.HttpContext.Request;
                        var url = contextualize.Url.Action(routes["action"].ToString(), routes["controller"].ToString(), routes, request.Scheme);

                        context.Builder.AddProperty(context, feedItem, "link", url);
                    });

                    context.Builder.AddProperty(context, feedItem, "title", contentItemMetadata.DisplayText);
                    context.Builder.AddProperty(context, feedItem, "description", bodyAspect.Body.ToString());

                    if (contentItem.PublishedUtc != null)
                    {
                        context.Builder.AddProperty(context, feedItem, "published-date", contentItem.PublishedUtc.Value.ToString("r"));
                    }
                }
            }
        }
    }
}