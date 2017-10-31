using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.Client.Abstractions;

namespace OrchardCore.GraphQL.Client
{
    public class TenantResource : ITenantResource
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
            var variables =
@"{ 
    ""site"": {
        ""siteName"": """ + siteName + @""",
        ""databaseProvider"": """+ databaseProvider + @""",
        ""userName"": """+userName+ @""",
        ""email"": """ + email + @""",
        ""password"": """ + password + @""",
        ""passwordConfirmation"": """ + password + @""",
        ""recipeName"": """ + recipeName + @"""
    }
}";

            var json = @"{
  ""query"": ""mutation ($site: SiteSetupInput!){ createSite(site: $site) { executionId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _client.PostJsonAsync("graphql", json);

            response.EnsureSuccessStatusCode();

            var value = await response.Content.ReadAsStringAsync();

            return JObject.Parse(value)["data"]["createSite"]["executionId"].ToString();
        }
    }
}
