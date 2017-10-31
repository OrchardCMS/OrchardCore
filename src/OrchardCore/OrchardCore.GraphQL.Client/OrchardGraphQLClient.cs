using System.Net.Http;
using OrchardCore.Apis.Client.Abstractions;

namespace OrchardCore.GraphQL.Client
{
    public class OrchardGraphQLClient : IApiClient
    {
        private readonly HttpClient _client;

        public OrchardGraphQLClient(HttpClient client)
        {
            _client = client;
        }

        public ITenantResource Tenants => new TenantResource(_client);

        public IContentResource Content => new ContentResource(_client);
    }
}
