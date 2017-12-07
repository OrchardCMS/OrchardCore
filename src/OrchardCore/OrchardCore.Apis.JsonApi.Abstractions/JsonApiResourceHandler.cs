using System.Collections.Generic;
using System.Linq;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public abstract class JsonApiResourceHandler<TSource> : IJsonApiResourceHandler<TSource>
    {
        public virtual IEnumerable<ApiProperty> BuildAttributes(IUrlHelper urlHelper, TSource item)
        {
            return Enumerable.Empty<ApiProperty>();
        }

        public virtual void UpdateLinks(Links links, IUrlHelper urlHelper, TSource item)
        {
        }

        public virtual void UpdateRelationships(IDictionary<string, Relationship> relationships, IUrlHelper urlHelper, TSource item)
        {
        }
    }
}
