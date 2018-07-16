using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Apis.JsonApi.Client
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
                new JProperty(
                    "data", 
                    new JObject(
                        new JProperty("type", "setup"),
                        new JProperty("attributes", 
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
                    )
                )
            );

            var response = await _client.PostJsonApiAsync("api/setup", requestJson.ToString());

            response.EnsureSuccessStatusCode();

            await _client.GetAsync("");

            return await response.Content.ReadAsAsync<string>();
        }
    }
}