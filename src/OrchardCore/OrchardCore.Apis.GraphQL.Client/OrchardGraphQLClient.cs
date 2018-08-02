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

        public TenantResource Tenants => new TenantResource(_client);

        public ContentResource Content => new ContentResource(_client);
    }
}
