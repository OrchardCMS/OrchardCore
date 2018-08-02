using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class ContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> Create(string contentType, Action<ContentTypeCreateResourceBuilder> builder) {
            var contentTypeBuilder = new ContentTypeCreateResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var variables = contentTypeBuilder.Build();

            var requestJson = new JObject(
                new JProperty("query", "mutation { createContentItem( ContentItem: { " + variables + " } ) { contentItemId } }"));

            var response = await _client.PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            return result["data"]["createContentItem"]["contentItemId"].ToString();
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

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task Delete(string blogPostContentItemId)
        {
            var requestJson = new JObject(
                new JProperty("query", @"mutation DeleteContentItem { deleteContentItem( ContentItemId: """+ blogPostContentItemId + @""") { status } }")
                );

            var response = await _client.PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<JObject> NamedQueryExecute(string name)
        {
            var requestJson = new JObject(
                new JProperty("namedquery", name)
                );

            var response = await _client.PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public QueriesResource Queries => new QueriesResource(_client);
    }
}
