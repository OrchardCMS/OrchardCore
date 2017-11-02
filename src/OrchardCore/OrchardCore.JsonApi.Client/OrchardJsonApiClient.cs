using System.Net.Http;

namespace OrchardCore.JsonApi.Client
{
    public class OrchardJsonApiClient
    {
        private readonly HttpClient _client;

        public OrchardJsonApiClient(HttpClient client)
        {
            _client = client;
        }

        public TenantResource Tenants => new TenantResource(_client);

        public ContentResource Content => new ContentResource(_client);
    }
}
