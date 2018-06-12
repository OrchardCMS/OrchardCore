using System;
using System.Net.Http;
using System.Threading.Tasks;
using OrchardCore.Apis.JsonApi.Client.Builders;

namespace OrchardCore.Apis.JsonApi.Client
{
    public class ContentResource
    {
        private HttpClient _client;

        public ContentResource(HttpClient client)
        {
            _client = client;
        }

        public Task<string> Create(string contentType, Action<ContentTypeCreateResourceBuilder> builder)
        {
            var contentTypeBuilder = new ContentTypeCreateResourceBuilder(contentType);
            builder(contentTypeBuilder);

            var variables = contentTypeBuilder.Build();


            return Task.FromResult("");
        }
    }
}