using System.Collections.Generic;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public interface IJsonApiResourceHandler<TSource>
    {
        void UpdateLinks(Links links, IUrlHelper urlHelper, TSource item);
        void UpdateRelationships(IDictionary<string, Relationship> relationships, IUrlHelper urlHelper, TSource item);
        IEnumerable<ApiProperty> BuildAttributes(IUrlHelper urlHelper, TSource item);
    }
}
