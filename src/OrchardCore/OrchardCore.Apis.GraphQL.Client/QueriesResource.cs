using System.Net.Http;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class QueriesResource
    {
        private HttpClient _client;

        public QueriesResource(HttpClient client)
        {
            _client = client;
        }
    }
}