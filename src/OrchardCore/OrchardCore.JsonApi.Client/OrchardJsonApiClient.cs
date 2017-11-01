using System;
using System.Net.Http;
using OrchardCore.Apis.Client.Abstractions;

namespace OrchardCore.JsonApi.Client
{
    public class OrchardJsonApiClient : IApiClient
    {
        private readonly HttpClient _client;

        public OrchardJsonApiClient(HttpClient client)
        {
            _client = client;
        }

        public ITenantResource Tenants => new TenantResource(_client);

        public IContentResource Content => new ContentResource(_client);
    }
}
