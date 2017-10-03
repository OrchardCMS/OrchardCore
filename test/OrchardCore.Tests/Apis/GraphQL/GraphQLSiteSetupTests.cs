using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Tests.Apis.Sources;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class GraphQLSiteSetupTests : IDisposable
    {
        private OrchardTestFixture<SiteStartup> _site;

        public GraphQLSiteSetupTests()
        {
            _site = new OrchardTestFixture<SiteStartup>(EnvironmentHelpers.GetApplicationPath());
        }

        [Fact]
        public async Task ShouldSetSiteupUsingSqlite()
        {
            var variables =
@"{ 
    ""site"": {
        ""siteName"": """ + _site.SiteName + @""",
        ""databaseProvider"": ""Sqlite"",
        ""userName"": ""admin"",
        ""email"": ""fu@bar.com"",
        ""password"": ""Password01_"",
        ""passwordConfirmation"": ""Password01_"",
        ""recipeName"": ""Blog""
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

        public void Dispose()
        {
            _site.Dispose();
        }
    }
}
