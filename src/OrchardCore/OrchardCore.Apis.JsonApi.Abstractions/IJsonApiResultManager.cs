using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public interface IJsonApiResultManager
    {
        Task<Document> Build(IUrlHelper urlHelper, object actionValue);
    }
}