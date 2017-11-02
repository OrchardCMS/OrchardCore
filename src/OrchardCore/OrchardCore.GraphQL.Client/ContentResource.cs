using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.GraphQL.Client
{
    public class ContentResource
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

            var requestJson = new JObject(
                new JProperty("query", "mutation ($contentItem: ContentItemInput!){ createContentItem(contentItem: $contentItem) { contentItemId } }"),
                new JProperty("variables", JsonConvert.SerializeObject(variables.ToString()))
                );

            var response = await _client.PostJsonAsync("graphql", requestJson.ToString());

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
            var requestJson = new JObject(
                new JProperty("query", "mutation DeleteContentItem { deleteContentItem( contentItemId: \""+ blogPostContentItemId + @"\"") { status } }"),
                new JProperty("variables", "")
                );

            var response = await _client.PostJsonAsync("graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
    }
}
