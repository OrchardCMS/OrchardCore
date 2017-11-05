using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class QueriesResource
    {
        private HttpClient _client;

        public QueriesResource(HttpClient client)
        {
            _client = client;
        }

        public async Task CreateSqlQuery(string template, string name)
        {
            var requestJson = new JObject(
                new JProperty("query", "mutation CreateSqlQuery { " +
                "createSqlQuery(" +
                @" Template: """ + template + @"""," +
                @" Name: """ + name + @"""" +
                " ) { source } }"),
                new JProperty("variables", "")
                );

            var response = await _client.PostJsonAsync("graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task Delete(string name)
        {
            var requestJson = new JObject(
                new JProperty("query", @"mutation DeleteQuery { deleteQuery( Name: """ + name + @""") { status } }"),
                new JProperty("variables", "")
                );

            var response = await _client.PostJsonAsync("graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }
    }
}