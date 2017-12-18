using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Title.Model;

namespace OrchardCore.Title.JsonApi
{
    public class TitleResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        public override Task<IEnumerable<ApiProperty>> BuildAttributes(
            IUrlHelper urlHelper, ContentItem item)
        {
            var part = item.As<TitlePart>();

            if (part == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty> {
                ApiProperty.Create(
                   typeof(TitlePart).Name,
                    new ApiObject(
                        ApiProperty.Create(nameof(part.Title), part.Title)
                    )
                )
            };

            return Task.FromResult<IEnumerable<ApiProperty>>(properties);
        }
    }
}
