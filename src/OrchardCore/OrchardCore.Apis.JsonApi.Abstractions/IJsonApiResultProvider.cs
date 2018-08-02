using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public interface IJsonApiResultProvider
    {
        int Order { get; }
        Task<Document> Build(IUrlHelper urlHelper, object actionValue);
    }

    public abstract class JsonApiResultProvider : IJsonApiResultProvider
    {
        public virtual int Order => 0;

        public virtual Task<Document> Build(IUrlHelper urlHelper, object actionValue)
        {
            return Task.FromResult<Document>(null);
        }
    }
}
