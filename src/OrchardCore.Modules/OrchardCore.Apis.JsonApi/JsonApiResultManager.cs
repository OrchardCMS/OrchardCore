using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Apis.JsonApi
{
    public class JsonApiResultManager : IJsonApiResultManager
    {
        private readonly IEnumerable<IJsonApiResultProvider> _providers;

        public JsonApiResultManager(IEnumerable<IJsonApiResultProvider> providers)
        {
            _providers = providers.OrderBy(provider => provider.Order).ToList();
        }

        public async Task<Document> Build(IUrlHelper urlHelper, object actionValue)
        {
            foreach (var provider in _providers)
            {
                var result = await provider.Build(urlHelper, actionValue);

                if (result != null)
                {
                    return result;
                }
            }

            return Document.Empty;
        }
    }
}
