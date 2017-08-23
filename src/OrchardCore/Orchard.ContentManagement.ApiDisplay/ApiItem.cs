using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonApiSerializer.JsonApi;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.Api
{
    public class ApiItem
    {
        private readonly IUrlHelper _urlHelper;

        public ApiItem(
            ContentItem contentItem,
            IUrlHelper urlHelper)
            //IUrlHelper urlHelper,
            //List<ContentPart> contentParts)
        {
            _urlHelper = urlHelper;

            Type = contentItem.ContentType;
            Id = contentItem.ContentItemId;
            VersionId = contentItem.ContentItemVersionId;
            Number = contentItem.Number;
            Published = contentItem.Published;
            Latest = contentItem.Latest;
            ModifiedUtc = contentItem.ModifiedUtc;
            PublishedUtc = contentItem.PublishedUtc;
            CreatedUtc = contentItem.CreatedUtc;
            Owner = contentItem.Owner;
            Author = contentItem.Author;

            Links.Add("self", new Link { Href = urlHelper.RouteUrl("Api_Self_Route", Id) });

            Links.Add("allversions", new Link
            {
                Href = _urlHelper.RouteUrl("Api_Self_Route_Version", new
                {
                    contentItemId = Id,
                    versionOptions = "IsAllVersions"
                })
            });
        }

        /// <summary>
        /// The content type of the content item.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The logical identifier of the content item across versions.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The logical identifier of the versioned content item.
        /// </summary>
        public string VersionId { get; set; }

        /// <summary>
        /// The number of the version.
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Whether the version is published or not.
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Whether the version is the latest version of the content item.
        /// </summary>
        public bool Latest { get; set; }

        /// <summary>
        /// When the content item version has been updated.
        /// </summary>
        public DateTime? ModifiedUtc { get; set; }

        /// <summary>
        /// When the content item has been published.
        /// </summary>
        public DateTime? PublishedUtc { get; set; }

        /// <summary>
        /// When the content item has been created or first published.
        /// </summary>
        public DateTime? CreatedUtc { get; set; }

        /// <summary>
        /// The name of the user who first created this content item version
        /// and owns content rigths.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The name of the user who last modified this content item version.
        /// </summary>
        public string Author { get; set; }

        public Links Links { get; internal set; } = new Links();

        public Meta Meta { get; internal set; } = new Meta();

        public void AddPart(ContentPart contentPart)
        {
            var token = JToken.FromObject(contentPart);

            if (token.HasValues)
            {
                Meta.Add(contentPart.GetType().Name, token);
            }
        }

        public void AddLatest(ContentItem contentItem)
        {
            Links.Add("latest", new Link
            {
                Href = _urlHelper.RouteUrl("Api_Self_Route_Version", new {
                    contentItemId = Id,
                    versionOptions = "IsLatest"
                })
            });
        }

        public void AddPublished(ContentItem contentItem)
        {
            Links.Add("published", new Link
            {
                Href = _urlHelper.RouteUrl("Api_Self_Route_Version", new
                {
                    contentItemId = Id,
                    versionOptions = "IsPublished"
                })
            });
        }
    }
}
