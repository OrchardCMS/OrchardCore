using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Tests.Apis.Sources;
using Xunit;

namespace OrchardCore.Tests.Apis
{
    public class GraphQLSiteSetupTests : IDisposable
    {
        private MvcTestFixture<SiteStartup> _site;

        public GraphQLSiteSetupTests()
        {
            var path = Path.Combine("src", "OrchardCore.Cms.Web");

            var appData = Path.Combine(EnvironmentHelpers.GetApplicationPath(), "App_Data");

            if (Directory.Exists(appData))
            {
                Directory.Delete(appData, true);
            }

            _site = new MvcTestFixture<SiteStartup>(path);
        }

        public void Dispose()
        {
            _site.Dispose();
        }

        [Fact]
        public async Task ShouldSetSiteupUsingSqlite()
        {
            string siteName = Guid.NewGuid().ToString().Replace("-", "");

            var variables =
@"{ 
    ""site"": {
        ""siteName"": """ + siteName + @""",
        ""databaseProvider"": ""Sqlite"",
        ""userName"": ""admin"",
        ""email"": ""fu@bar.com"",
        ""password"": ""Password01_"",
        ""passwordConfirmation"": ""Password01_"",
        ""recipeName"": ""blog.recipe.json""
    }
}";

            var json = @"{
  ""query"": ""mutation ($site: SiteSetupInput!){ createSite(site: $site) { executionId } }"",
  ""variables"": " + JsonConvert.SerializeObject(variables) + @"}";

            var response = await _site.Client.PostJsonAsync("graphql", json);

            response.EnsureSuccessStatusCode();

            var value = await response.Content.ReadAsStringAsync();

            Assert.Equal(32, JObject.Parse(value)["data"]["createSite"]["executionId"].Value<string>().Length);
        }
    }
}
