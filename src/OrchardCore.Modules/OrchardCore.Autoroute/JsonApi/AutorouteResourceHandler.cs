using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Apis.JsonApi;
using OrchardCore.ContentManagement;
using OrchardCore.Autoroute.Model;

namespace OrchardCore.Autoroute.JsonApi
{
    public class AutorouteResourceHandler : JsonApiResourceHandler<ContentItem>
    {
        public override Task<IEnumerable<ApiProperty>> BuildAttributes(
            IUrlHelper urlHelper, ContentItem item)
        {
            var part = item.As<AutoroutePart>();

            if (part == null)
            {
                return base.BuildAttributes(urlHelper, item);
            }

            var properties = new List<ApiProperty> {
                ApiProperty.Create(
                   typeof(AutoroutePart).Name,
                    new ApiObject(
                        ApiProperty.Create(nameof(part.Path), part.Path),
                        ApiProperty.Create(nameof(part.SetHomepage), part.SetHomepage)
                    )
                )
            };

            return Task.FromResult<IEnumerable<ApiProperty>>(properties);
        }
    }
}
