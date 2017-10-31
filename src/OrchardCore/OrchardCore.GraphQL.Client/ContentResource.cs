using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.Client.Abstractions;

namespace OrchardCore.GraphQL.Client
{
    public class ContentResource : IContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> CreateAsync(string contentType, Action<ContentTypeCreateResourceBuilder> builder) {
            var contentTypeBuilder = new ContentTypeCreateResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var variables = contentTypeBuilder.Build();

            var json = @"{
  ""query"": ""mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { contentItemId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _client.PostJsonAsync("graphql", json);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            return result["data"]["createContentItem"]["contentItemId"].ToString();
        }

        public async Task<JObject> QueryAsync(string contentType, Action<ContentTypeQueryResourceBuilder> builder)
        {
            var contentTypeBuilder = new ContentTypeQueryResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var variables = "query { " + contentTypeBuilder.Build() + " }";

            var response = await _client
                .GetAsync("graphql?query=" + HttpUtility.UrlEncode(variables));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task DeleteAsync(string blogPostContentItemId)
        {
            var json = @"{
  ""query"": ""mutation DeleteContentItem { deleteContentItem( contentItemId: \"""+ blogPostContentItemId + @"\"" ) { status } }"",
  ""variables"": """" }";

            var response = await _client.PostJsonAsync("graphql", json);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
