using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.JsonApi
{
    public class ContainedResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        private readonly IContentManager _contentManager;

        public ContainedResourceHandler(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        //public override void UpdateLinks(Links links, IUrlHelper urlHelper, ContentItem item)
        //{
        //    var containedPart = item.As<ContainedPart>();

        //    if (containedPart == null)
        //    {
        //        return;
        //    }

        //    links.Add(
        //        "parent",
        //        urlHelper.RouteUrl(
        //            "Api.GetContents.ById",
        //            new { area = "Orchard.Contents", contentItemId = containedPart.ListContentItemId })
        //        );
        //}

        public override Task<IEnumerable<ApiProperty>> BuildAttributes(IUrlHelper urlHelper, ContentItem item)
        {
            var containedPart = item.As<ContainedPart>();

            if (containedPart == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty> {
                ApiProperty.Create(
                   typeof(ContainedPart).Name,
                    new ApiObject(
                        ApiProperty.Create("ListContentItemId", containedPart.ListContentItemId),
                        ApiProperty.Create("Order", containedPart.Order)
                    )
                )
            };

            return Task.FromResult<IEnumerable<ApiProperty>>(properties);
        }

        public override async Task UpdateRelationships(
            IDictionary<string, Relationship> relationships, 
            IUrlHelper urlHelper, 
            ContentItem item)
        {
            var containedPart = item.As<ContainedPart>();

            if (containedPart == null)
            {
                return;
            }

            var parentContentItem = await _contentManager.GetAsync(containedPart.ListContentItemId);

            relationships.Add(parentContentItem.ContentType, new ToOneRelationship
            {
                Links = new Links {
                    {
                        Keywords.Related,
                        urlHelper.RouteUrl(
                            "Api.GetContents.ById",
                            new { area = "Orchard.Contents", contentItemId = containedPart.ListContentItemId })
                    }
                }
            });
        }
    }
}
