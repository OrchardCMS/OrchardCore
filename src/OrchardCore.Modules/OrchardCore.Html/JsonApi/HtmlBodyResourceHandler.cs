using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Html.Model;

namespace OrchardCore.Html.JsonApi
{
    public class HtmlBodyResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        public override Task<IEnumerable<ApiProperty>> BuildAttributes(
            IUrlHelper urlHelper, ContentItem item)
        {
            var part = item.As<HtmlBodyPart>();

            if (part == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty> {
                ApiProperty.Create(
                   typeof(HtmlBodyPart).Name,
                    new ApiObject(
                        ApiProperty.Create(nameof(part.Html), part.Html)
                    )
                )
            };

            return Task.FromResult<IEnumerable<ApiProperty>>(properties);
        }
    }
}
