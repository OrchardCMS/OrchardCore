using System.Net.Http;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class OrchardGraphQLClient
    {
        private readonly HttpClient _client;

        public OrchardGraphQLClient(HttpClient client)
        {
            _client = client;
        }

        public ContentResource Content => new ContentResource(_client);

        public HttpClient Client => _client;
    }
}
