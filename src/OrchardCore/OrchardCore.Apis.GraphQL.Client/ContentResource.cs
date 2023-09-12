using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentResource
    {
        private readonly HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<JObject> Query(string contentType, Action<ContentTypeQueryResourceBuilder> builder)
        {
            var contentTypeBuilder = new ContentTypeQueryResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var requestJson = new JObject(
                new JProperty("query", @"query { " + contentTypeBuilder.Build() + " }")
                );

            var response = await _client
                .PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception(response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JObject> Query(string body)
        {
            var requestJson = new JObject(
                new JProperty("query", @"query { " + body + " }")
                );

            var response = await _client.PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task<JObject> NamedQueryExecute(string name)
        {
            var requestJson = new JObject(
                new JProperty("namedquery", name)
                );

            var response = await _client.PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.StatusCode.ToString() + " " + await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }
    }
}
