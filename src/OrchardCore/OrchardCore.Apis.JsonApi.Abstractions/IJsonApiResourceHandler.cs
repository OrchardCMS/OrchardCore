using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public interface IJsonApiResourceHandler<TSource>
    {
        Task UpdateLinks(Links links, IUrlHelper urlHelper, TSource item);
        Task UpdateRelationships(IDictionary<string, Relationship> relationships, IUrlHelper urlHelper, TSource item);
        Task<IEnumerable<ApiProperty>> BuildAttributes(IUrlHelper urlHelper, TSource item);
    }
}
