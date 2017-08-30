using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JsonApiSerializer.JsonApi;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.ContentManagement;

namespace Orchard.JsonApi
{
    public class ApiItem
    {
        private readonly IUrlHelper _urlHelper;

        public ApiItem()
        { }

        public ApiItem(
            ContentItem contentItem,
            IUrlHelper urlHelper = null) : base()
        {
            if (string.IsNullOrWhiteSpace(contentItem.ContentItemId))
            {
                throw new ArgumentNullException(nameof(contentItem.ContentItemId));
            }

            if (string.IsNullOrWhiteSpace(contentItem.ContentType))
            {
                throw new ArgumentNullException(nameof(contentItem.ContentType));
            }

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

            if (urlHelper != null)
            {
                Links = new Links
                {
                    { "self", new Link { Href = urlHelper.RouteUrl("Api.GetContents.ByTypeAndId", new { area = "Orchard.Contents", contentType = Type, contentItemId = Id }) } }
                };
            }
        }

        /// <summary>
        /// The logical identifier of the content item across versions.
        /// </summary>
        [DataMember(Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// The content type of the content item.
        /// </summary>
        [DataMember(Order = 2)]
        public string Type { get; set; }

        /// <summary>
        /// The logical identifier of the versioned content item.
        /// </summary>
        [DataMember(Order = 3)]
        public string VersionId { get; set; }

        /// <summary>
        /// The number of the version.
        /// </summary>
        [DataMember(Order = 4)]
        public int Number { get; set; }

        /// <summary>
        /// Whether the version is published or not.
        /// </summary>
        [DataMember(Order = 5)]
        public bool Published { get; set; }

        /// <summary>
        /// Whether the version is the latest version of the content item.
        /// </summary>
        [DataMember(Order = 6)]
        public bool Latest { get; set; }

        /// <summary>
        /// When the content item version has been updated.
        /// </summary>
        [DataMember(Order = 7)]
        public DateTime? ModifiedUtc { get; set; }

        /// <summary>
        /// When the content item has been published.
        /// </summary>
        [DataMember(Order = 8)]
        public DateTime? PublishedUtc { get; set; }

        /// <summary>
        /// When the content item has been created or first published.
        /// </summary>
        [DataMember(Order = 9)]
        public DateTime? CreatedUtc { get; set; }

        /// <summary>
        /// The name of the user who first created this content item version
        /// and owns content rigths.
        /// </summary>
        [DataMember(Order = 10)]
        public string Owner { get; set; }

        /// <summary>
        /// The name of the user who last modified this content item version.
        /// </summary>
        [DataMember(Order = 11)]
        public string Author { get; set; }

        public Dictionary<string, JToken> ContentParts { get; set; }

        public Links Links { get; }

        //public Meta Meta { get; internal set; } = new Meta();

        public Relationship<List<ApiRelationshipItem>> ContentItems { get; set; }

        public void AddPart(ContentPart contentPart)
        {
            var token = JToken.FromObject(contentPart);

            if (token.HasValues)
            {
                if (ContentParts == null)
                {
                    ContentParts = new Dictionary<string, JToken>();
                }

                ContentParts.Add(contentPart.GetType().Name, token);
            }
        }

        public void AddRelationship(ApiRelationshipItem relationship)
        {
            if (ContentItems == null)
            {
                ContentItems = new Relationship<List<ApiRelationshipItem>>();
                ContentItems.Data = new List<ApiRelationshipItem>();
            }

            ContentItems.Data.Add(relationship);

            ContentItems.Links = relationship.Links;

            if (relationship.Meta.Count > 0)
            {
                ContentItems.Meta = relationship.Meta;
            }
        }

        //public void AddLatest(ContentItem contentItem)
        //{
        //    Links.Add("latest", new Link
        //    {
        //        Href = _urlHelper.RouteUrl("Api_Self_Route_Version", new {
        //            contentItemId = Id,
        //            versionOptions = "IsLatest"
        //        })
        //    });
        //}

        //public void AddPublished(ContentItem contentItem)
        //{
        //    Links.Add("published", new Link
        //    {
        //        Href = _urlHelper.RouteUrl("Api_Self_Route_Version", new
        //        {
        //            contentItemId = Id,
        //            versionOptions = "IsPublished"
        //        })
        //    });
        //}
    }

    public class ApiRelationshipItem {
        public ApiRelationshipItem(
            ContentItem parentContentItem,
            ContentItem contentItem,
            IUrlHelper urlHelper = null)
        {
            Type = contentItem.ContentType;
            Id = contentItem.ContentItemId;

            if (urlHelper != null)
            {
                Links = new Links
                {
                    { "self", new Link {
                        Href = urlHelper.RouteUrl("Api.GetContents.ByRelationship", new {
                            area = "Orchard.Contents",
                            contentType = parentContentItem.ContentType,
                            contentItemId = parentContentItem.ContentItemId,
                            nestedContentType = Type
                        })
                    } }
                };
            }

            Meta = new Meta();
        }

        /// <summary>
        /// The logical identifier of the content item across versions.
        /// </summary>
        [DataMember(Order = 1)]
        public string Id { get; set; }

        /// <summary>
        /// The content type of the content item.
        /// </summary>
        [DataMember(Order = 2)]
        public string Type { get; set; }

        [JsonIgnore]
        public Links Links { get; }
        
        [JsonIgnore]
        public Meta Meta { get; }
    }
}
