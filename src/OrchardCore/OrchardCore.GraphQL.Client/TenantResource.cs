using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OrchardCore.GraphQL.Client
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
            var variables = new JObject(
                new JProperty(
                    "site",
                    new JObject(
                        new JProperty("siteName", siteName),
                        new JProperty("databaseProvider", databaseProvider),
                        new JProperty("userName", userName),
                        new JProperty("email", email),
                        new JProperty("password", password),
                        new JProperty("passwordConfirmation", password),
                        new JProperty("recipeName", recipeName)
                    )
                )
            );

            var requestJson = new JObject(
                new JProperty("query", "mutation ($site: SiteSetupInput!){ createSite(site: $site) { executionId } }"),
                new JProperty("variables", variables.ToString())
                );

            var response = await _client.PostJsonAsync("graphql", requestJson.ToString());

            response.EnsureSuccessStatusCode();

            var value = await response.Content.ReadAsStringAsync();

            return JObject.Parse(value)["data"]["createSite"]["executionId"].ToString();
        }
    }
}
