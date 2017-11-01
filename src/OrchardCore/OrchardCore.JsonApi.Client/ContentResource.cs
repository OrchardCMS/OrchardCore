using System.Net.Http;
using OrchardCore.Apis.Client.Abstractions;

namespace OrchardCore.JsonApi.Client
{
    internal class ContentResource : IContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }
    }
}