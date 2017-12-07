using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.JsonApi
{
    public class ContentJsonApiResultManager : IJsonApiResultManager
    {
        private readonly IEnumerable<IJsonApiResourceHandler<ContentItem>> _handlers;

        private readonly static JsonApiVersion Version = JsonApiVersion.Version11;

        public ContentJsonApiResultManager(IEnumerable<IJsonApiResourceHandler<ContentItem>> handlers)
        {
            _handlers = handlers;
        }

        public Task<Document> Build(IUrlHelper urlHelper, object actionValue)
        {
            var contentItem = actionValue as ContentItem;

            if (contentItem != null)
            {
                return BuildDocument(urlHelper, contentItem);
            }

            var contentItems = actionValue as IEnumerable<ContentItem>;

            if (contentItem != null)
            {
                return BuildDocumentCollection(urlHelper, contentItems.ToArray());
            }

            contentItems = actionValue as ContentItem[];

            if (contentItems != null)
            {
                return BuildDocumentCollection(urlHelper, contentItems.ToArray());
            }

            return Task.FromResult<Document>(null);
        }

        public Task<Document> BuildDocumentCollection(IUrlHelper urlHelper, ContentItem[] contentItems)
        {
            var resourceDocument = new ResourceCollectionDocument
            {
                Links = new Links(), // TODO: Links for collections
                Data = contentItems.Select(ci => BuildContentItemData(urlHelper, ci)).ToList(),
                JsonApiVersion = Version
            };

            return Task.FromResult<Document>(resourceDocument);
        }

        public Task<Document> BuildDocument(IUrlHelper urlHelper, ContentItem contentItem)
        {
            var resourceDocument = new ResourceDocument
            {
                Links = BuildContentItemLinks(urlHelper, contentItem),
                Data = BuildContentItemData(urlHelper, contentItem),
                JsonApiVersion = Version
            };

            return Task.FromResult<Document>(resourceDocument);
        }

        private Resource BuildContentItemData(IUrlHelper urlHelper, ContentItem contentItem)
        {
            return new Resource
            {
                Type = contentItem.ContentType,
                Id = contentItem.ContentItemId,
                Attributes = BuildContentItemAttributes(urlHelper, contentItem),
                Relationships = BuildContentItemRelationships(urlHelper, contentItem),
                Links = BuildContentItemLinks(urlHelper, contentItem)
            };
        }

        private Links BuildContentItemLinks(IUrlHelper urlHelper, ContentItem contentItem)
        {
            var links = new Links {
                    {
                        Keywords.Self,
                        urlHelper.RouteUrl(
                            RouteHelpers.ApiRouteByIdName,
                            new { area = RouteHelpers.AreaName, contentItemId = contentItem.ContentItemId })
                    },
                    {
                        Keywords.Version,
                        urlHelper.RouteUrl(
                            RouteHelpers.ApiRouteByVersionName,
                            new { area = RouteHelpers.AreaName, contentItemVersionId = contentItem.ContentItemVersionId })
                    }
                };

            // TODO: Need to add interface to allow parts to attach to it too

            foreach (var handler in _handlers)
            {
                handler.UpdateLinks(links, urlHelper, contentItem);
            }

            return links;
        }

        private Relationships BuildContentItemRelationships(IUrlHelper urlHelper, ContentItem contentItem)
        {
            IDictionary<string, Relationship> relationships = new Dictionary<string, Relationship>();

            foreach (var handler in _handlers)
            {
                handler.UpdateRelationships(relationships, urlHelper, contentItem);
            }

            return new Relationships(relationships);
        }

        private ApiObject BuildContentItemAttributes(IUrlHelper urlHelper, ContentItem contentItem)
        {
            var properties = new List<ApiProperty> {
                ApiProperty.Create("ContentItemVersionId", contentItem.ContentItemVersionId),
                ApiProperty.Create("Published", contentItem.Published),
                ApiProperty.Create("Latest", contentItem.Latest),
                ApiProperty.Create("ModifiedUtc", contentItem.ModifiedUtc),
                ApiProperty.Create("CreatedUtc", contentItem.CreatedUtc),
                ApiProperty.Create("PublishedUtc", contentItem.PublishedUtc),
                ApiProperty.Create("Owner", contentItem.Owner),
                ApiProperty.Create("Author", contentItem.Author)
            };

            foreach (var handler in _handlers)
            {
                properties.AddRange(handler.BuildAttributes(urlHelper, contentItem));
            }

            return new ApiObject(properties);
        }
    }
}
