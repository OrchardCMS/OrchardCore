using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.Client.Abstractions;

namespace OrchardCore.JsonApi.Client
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
            string json = @"{
  ""data"": {
    ""type"": ""setup"",
    ""attributes"": {
        ""siteName"": """ + siteName + @""",
        ""databaseProvider"": """ + databaseProvider + @""",
        ""userName"": """ + userName + @""",
        ""email"": """ + email + @""",
        ""password"": """+password+ @""",
        ""passwordConfirmation"": """ + password + @""",
        ""recipeName"": """ + recipeName + @"""
    }
  }
}";

            var response = await _client.PostJsonApiAsync("/", json);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsAsync<string>();
        }
    }
}