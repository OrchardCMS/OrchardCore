using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentResource
    {
        private readonly HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<JsonObject> Query(string contentType, Action<ContentTypeQueryResourceBuilder> builder)
        {
            var contentTypeBuilder = new ContentTypeQueryResourceBuilder(contentType);
            builder(contentTypeBuilder);

            return await PostQueryAsync(GetQueryJson(contentTypeBuilder.Build()));
        }

        public async Task<JsonObject> Query(string body) =>
            await PostQueryAsync(GetQueryJson(body));

        public async Task<JsonObject> NamedQueryExecute(string name)
        {
            var requestJson = new JsonObject
            {
                ["namedquery"] = name,
            };

            return await PostQueryAsync(requestJson.ToString());
        }

        private async Task<JsonObject> PostQueryAsync(string query)
        {
            var response = await _client.PostJsonAsync("api/graphql", query);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.StatusCode + " " + await response.Content.ReadAsStringAsync());
            }

            return JsonNode.Parse(await response.Content.ReadAsStringAsync()) as JsonObject;
        }

        private static string GetQueryJson(string query)
        {
            var jsonObject = new JsonObject
            {
                ["query"] = "query { " + query + " }",
            };

            return jsonObject.ToString();
        }
    }
}
