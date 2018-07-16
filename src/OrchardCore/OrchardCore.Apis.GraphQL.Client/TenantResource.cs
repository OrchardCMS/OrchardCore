using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.GraphQL.Client
{
    public class TenantResource
    {
        private HttpClient _client;

        public TenantResource(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> CreateTenant(
            string siteName,
            string databaseProvider,
            string userName,
            string password,
            string email,
            string recipeName)
        {
            var requestJson = new JObject(
                new JProperty("query", "mutation CreateTenant { " +
                "createTenant(" +
                @" SiteName: """ + siteName + @"""," +
                @" DatabaseProvider: """ + databaseProvider + @"""," +
                @" UserName: """ + userName + @"""," +
                @" Email: """ + email + @"""," +
                @" Password: """ + password + @"""," +
                @" PasswordConfirmation: """ + password + @"""," +
                @" RecipeName: """ + recipeName + @"""" +
                " ) { executionId } }")
                );

            var response = await _client.PostJsonAsync("api/graphql", requestJson.ToString());

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            var result = JObject.Parse(await response.Content.ReadAsStringAsync());

            await _client.GetAsync("");

            return result["data"]["createTenant"]["executionId"].ToString();
        }
    }
}
