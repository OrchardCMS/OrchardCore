using System.Net.Http;

namespace OrchardCore.Apis.JsonApi.Client
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
