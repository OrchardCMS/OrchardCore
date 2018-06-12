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

            response.EnsureSuccessStatusCode();

            var value = await response.Content.ReadAsStringAsync();

            return JObject.Parse(value)["data"]["createTenant"]["executionId"].ToString();
        }
    }
}
