using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Body.Model;

namespace OrchardCore.Body.JsonApi
{
    public class BodyResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        public override Task<IEnumerable<ApiProperty>> BuildAttributes(
            IUrlHelper urlHelper, ContentItem item)
        {
            var part = item.As<BodyPart>();

            if (part == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty> {
                ApiProperty.Create(
                   typeof(BodyPart).Name,
                    new ApiObject(
                        ApiProperty.Create(nameof(part.Body), part.Body)
                    )
                )
            };

            return Task.FromResult<IEnumerable<ApiProperty>>(properties);
        }
    }
}
