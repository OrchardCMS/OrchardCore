using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Alias.Models;

namespace OrchardCore.Alias.JsonApi
{
    public class AliasResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        public override Task<IEnumerable<ApiProperty>> BuildAttributes(
            IUrlHelper urlHelper, ContentItem item)
        {
            var part = item.As<AliasPart>();

            if (part == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty> {
                ApiProperty.Create(
                   typeof(AliasPart).Name,
                    new ApiObject(
                        ApiProperty.Create(nameof(part.Alias), part.Alias)
                    )
                )
            };

            return Task.FromResult<IEnumerable<ApiProperty>>(properties);
        }
    }
}
