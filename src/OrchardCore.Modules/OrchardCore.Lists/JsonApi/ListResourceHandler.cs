using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Indexes;
using OrchardCore.Lists.Models;
using YesSql;

namespace OrchardCore.Lists.JsonApi
{
    public class ListResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        private readonly IContentManager _contentManager;
        private readonly ISession _session;

        public ListResourceHandler(IContentManager contentManager,
            ISession session)
        {
            _contentManager = contentManager;
            _session = session;
        }

        public override async Task UpdateRelationships(
            IDictionary<string, Relationship> relationships,
            IUrlHelper urlHelper,
            ContentItem item)
        {
            var listPart = item.As<ListPart>();

            if (listPart == null)
            {
                return;
            }

            var contentItems = await _session
                .Query<ContentItem, ContainedPartIndex>()
                .Where(x => x.ListContentItemId == listPart.ContentItem.ContentItemId)
                .ListAsync();

            foreach (var groupedContentItem in contentItems
                .GroupBy(ci => ci.ContentType))
            {

                var contentType = groupedContentItem.Key;
                var contentItemIds = groupedContentItem.Select(ci => ci.ContentItemId).ToList();

                relationships.Add(contentType, new ToManyRelationship
                {
                    Links = new Links {
                        {
                            Keywords.Self,
                            urlHelper.RouteUrl(
                                RouteHelpers.ContentItems.ApiRouteRelationshipByIdAndTypeName,
                                new { area = RouteHelpers.AreaName, contentItemId = item.ContentItemId, contentType = contentType })
                        }
                    },
                    Data = new List<ResourceIdentifier>(contentItemIds
                        .Select(cii => new ResourceIdentifier(contentType, cii)))
                });
            }
        }
    }
}
