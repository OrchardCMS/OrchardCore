using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Apis.GraphQL.Mutations
{
    public interface IApiUpdateModel : IUpdateModel
    {
        IApiUpdateModel WithModel(JObject jObject);
    }
}
