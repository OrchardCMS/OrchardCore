using System.Collections.Generic;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.JsonApi
{
    public class ContainedResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        public override void UpdateLinks(Links links, IUrlHelper urlHelper, ContentItem item)
        {
            var containedPart = item.As<ContainedPart>();

            if (containedPart == null)
            {
                return;
            }

            links.Add(
                "parent",
                urlHelper.RouteUrl(
                    "Api.GetContents.ById",
                    new { area = "Orchard.Contents", contentItemId = containedPart.ListContentItemId })
                );
        }

        public override IEnumerable<ApiProperty> BuildAttributes(IUrlHelper urlHelper, ContentItem item)
        {
            var containedPart = item.As<ContainedPart>();

            if (containedPart == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty>();

            properties.Add(ApiProperty.Create(
                typeof(ContainedPart).Name,
                new ApiObject(
                    ApiProperty.Create("Order", containedPart.Order)
                )
            ));

            return properties;
        }
    }
}
