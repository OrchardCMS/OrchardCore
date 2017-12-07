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
        private readonly IContentManager _contentManager;

        private readonly static JsonApiVersion Version = JsonApiVersion.Version11;

        public ContentJsonApiResultManager(
            IEnumerable<IJsonApiResourceHandler<ContentItem>> handlers,
            IContentManager contentManager)
        {
            _handlers = handlers;
            _contentManager = contentManager;
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
            // TODO : Proper async await

            var resourceDocument = new ResourceCollectionDocument
            {
                Links = new Links(), // TODO: Links for collections
                Data = contentItems.Select(ci => BuildContentItemData(urlHelper, ci).Result).ToList(),
                JsonApiVersion = Version
            };

            return Task.FromResult<Document>(resourceDocument);
        }

        public async Task<Document> BuildDocument(IUrlHelper urlHelper, ContentItem contentItem)
        {
            return new ResourceDocument
            {
                Links = await BuildContentItemLinks(urlHelper, contentItem),
                Data = await BuildContentItemData(urlHelper, contentItem),
                JsonApiVersion = Version
            };
        }

        private async Task<Resource> BuildContentItemData(IUrlHelper urlHelper, ContentItem contentItem)
        {
            return new Resource
            {
                Type = contentItem.ContentType,
                Id = contentItem.ContentItemId,
                Attributes = await BuildContentItemAttributes(urlHelper, contentItem),
                Relationships = await BuildContentItemRelationships(urlHelper, contentItem),
                Links = await BuildContentItemLinks(urlHelper, contentItem)
            };
        }

        private async Task<Links> BuildContentItemLinks(IUrlHelper urlHelper, ContentItem contentItem)
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

            if (contentItem.Latest)
            {
                links.Add(
                    LinkKeyworks.LatestVersion,
                    urlHelper.RouteUrl(
                        RouteHelpers.ApiRouteByIdName,
                        new { area = RouteHelpers.AreaName, contentItemId = contentItem.ContentItemId })
                    );
            }
            else {
                var latestContentItem = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Latest);

                links.Add(
                    LinkKeyworks.LatestVersion,
                    urlHelper.RouteUrl(
                        RouteHelpers.ApiRouteByVersionName,
                        new { area = RouteHelpers.AreaName, contentItemVersionId = latestContentItem.ContentItemVersionId })
                    );
            }

            if (contentItem.Published)
            {
                links.Add(
                    LinkKeyworks.PublishedVersion,
                    urlHelper.RouteUrl(
                        RouteHelpers.ApiRouteByIdName,
                        new { area = RouteHelpers.AreaName, contentItemId = contentItem.ContentItemId })
                    );
            }
            else {
                if (await _contentManager.HasPublishedVersionAsync(contentItem))
                {
                    var publishedContentItem = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Published);

                    links.Add(
                        LinkKeyworks.PublishedVersion,
                        urlHelper.RouteUrl(
                            RouteHelpers.ApiRouteByVersionName,
                            new { area = RouteHelpers.AreaName, contentItemVersionId = publishedContentItem.ContentItemVersionId })
                        );
                }
            }

            // TODO: Need to add interface to allow parts to attach to it too

            foreach (var handler in _handlers)
            {
                await handler.UpdateLinks(links, urlHelper, contentItem);
            }

            return links;
        }

        private async Task<Relationships> BuildContentItemRelationships(IUrlHelper urlHelper, ContentItem contentItem)
        {
            IDictionary<string, Relationship> relationships = new Dictionary<string, Relationship>();

            foreach (var handler in _handlers)
            {
                await handler.UpdateRelationships(relationships, urlHelper, contentItem);
            }

            return new Relationships(relationships);
        }

        private async Task<ApiObject> BuildContentItemAttributes(IUrlHelper urlHelper, ContentItem contentItem)
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
                properties.AddRange(await handler.BuildAttributes(urlHelper, contentItem));
            }

            return new ApiObject(properties);
        }
    }
}
