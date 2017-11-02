using System;
using System.Net.Http;
using System.Threading.Tasks;
using OrchardCore.JsonApi.Client.Builders;

namespace OrchardCore.JsonApi.Client
{
    public class ContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> Create(string contentType, Action<ContentTypeCreateResourceBuilder> builder)
        {

        }
    }
}