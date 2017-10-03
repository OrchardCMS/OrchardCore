using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Tests.Apis.Sources;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi
{
    public class JsonApiSiteSetupTests : IDisposable
    {
        private OrchardTestFixture<SiteStartup> _site;

        public JsonApiSiteSetupTests()
        {
            _site = new OrchardTestFixture<SiteStartup>(EnvironmentHelpers.GetApplicationPath());
        }

        [Fact]
        public async Task ShouldSetSiteupUsingSqlite()
        {
            string json = @"{
  ""data"": {
    ""type"": ""setup"",
    ""attributes"": {
        ""siteName"": """ + _site.SiteName + @""",
        ""databaseProvider"": ""Sqlite"",
        ""userName"": ""admin"",
        ""email"": ""fu@bar.com"",
        ""password"": ""Password01_"",
        ""passwordConfirmation"": ""Password01_"",
        ""recipeName"": ""Blog""
    }
  }
}";

            var response = await _site.Client.PostJsonApiAsync("/", json);

            response.EnsureSuccessStatusCode();

            var value = await response.Content.ReadAsStringAsync();

            //Assert.Equal(32, JObject.Parse(value)["data"]["createSite"]["executionId"].Value<string>().Length);
        }

        public void Dispose()
        {
            _site.Dispose();
        }
    }
}
