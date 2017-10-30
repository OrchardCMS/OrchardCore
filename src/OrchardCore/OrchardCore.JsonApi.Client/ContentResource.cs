using System.Net.Http;

namespace OrchardCore.JsonApi.Client
{
    internal class ContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }
    }
}