using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JsonApiFramework.JsonApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.JsonApi;
using OrchardCore.Queries.Helpers;

namespace OrchardCore.Queries.JsonApi
{
    public class QueriesJsonApiResultProvider : JsonApiResultProvider
    {
        private readonly static JsonApiVersion Version = JsonApiVersion.Version11;
        private readonly IActionContextAccessor _context;
        private readonly IQueryManager _queryManager;

        public QueriesJsonApiResultProvider(IActionContextAccessor context,
            IQueryManager queryManager)
        {
            _context = context;
            _queryManager = queryManager;
        }

        public override async Task<Document> Build(IUrlHelper urlHelper, object actionValue)
        {
            var context = _context.ActionContext;
            if (context.RouteData.DataTokens["area"].ToString() == RouteHelpers.AreaName)
            {
                var queryName = context.RouteData.Values["name"].ToString();
                var queryParameters = context.RouteData.Values["parameters"].ToString();

                // Get the Schema.
                var query = await _queryManager.GetQueryAsync(queryName);

                if (String.IsNullOrEmpty(query.Schema))
                {
                    return null;
                }

                var resourceCollection = new ResourceCollectionDocument {
                    JsonApiVersion = Version
                };

                var schema = JObject.Parse(query.Schema);

                var properties = schema["Properties"];

                var values = actionValue as IList<JObject>;

                if (values != null)
                {
                    foreach (var value in values)
                    {
                        var apiProperties = new List<ApiProperty>();

                        foreach (var child in properties.Children())
                        {
                            var name = ((JProperty)child).Name;
                            var nameLower = name.Replace('.', '_');
                            var type = child["type"].ToString();

                            apiProperties.Add(
                                ApiProperty.Create(name, Type.GetType(type))
                                );
                        }

                        var resource = new Resource {
                            Attributes = new ApiObject(apiProperties)
                        };

                        resourceCollection.AddResource(resource);
                    }
                }

                return resourceCollection;
            }

            throw new NotImplementedException();
        }

        public async Task<Document> BuildDocument(IUrlHelper urlHelper, JObject value)
        {
            return new ResourceDocument
            {
                Links = await BuildJObjectLinks(urlHelper, value),
                Data = await BuildJObjectData(urlHelper, value),
                JsonApiVersion = Version
            };
        }

        private Task<Links> BuildJObjectLinks(IUrlHelper urlHelper, JObject value)
        {
            return Task.FromResult(new Links());
        }

        private Task<Resource> BuildJObjectData(IUrlHelper urlHelper, JObject value)
        {
            var resource = new Resource
            {
                Type = "query"

            };

            return Task.FromResult(Resource.Empty);
        }
    }
}
