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
                var name = context.RouteData.Values["name"].ToString();
                var parameters = context.RouteData.Values["parameters"].ToString();

                // Get the Schema.
                var query = await _queryManager.GetQueryAsync(name);

                if (String.IsNullOrEmpty(query.Schema))
                {
                    return null;
                }

                var schema = JObject.Parse(query.Schema);

                //var type = schema["type"].ToString();

                //if (type.StartsWith("ContentItem/", System.StringComparison.OrdinalIgnoreCase))
                //{
                //    var contentType = type.Remove(0, 12);
                //    fieldTypes.Add(BuildContentTypeFieldType(state, contentType, query));
                //}
                //else
                //{
                //    fieldTypes.Add(BuildSchemaBasedFieldType(state, query, schema));
                //}

                var value = actionValue as JObject;

                if (value != null)
                {

                }

                var values = actionValue as IList<JObject>;

                if (values != null)
                {

                }
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
