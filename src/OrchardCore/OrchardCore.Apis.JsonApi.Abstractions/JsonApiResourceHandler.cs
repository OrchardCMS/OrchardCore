using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public abstract class JsonApiResourceHandler<TSource> : IJsonApiResourceHandler<TSource>
    {
        public virtual Task<IEnumerable<ApiProperty>> BuildAttributes(IUrlHelper urlHelper, TSource item)
        {
            return Task.FromResult(Enumerable.Empty<ApiProperty>());
        }

        public virtual Task UpdateLinks(Links links, IUrlHelper urlHelper, TSource item)
        {
            return Task.CompletedTask;
        }

        public virtual Task UpdateRelationships(IDictionary<string, Relationship> relationships, IUrlHelper urlHelper, TSource item)
        {
            return Task.CompletedTask;
        }
    }
}
