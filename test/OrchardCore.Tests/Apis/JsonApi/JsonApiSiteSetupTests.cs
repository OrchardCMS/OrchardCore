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
        private MvcTestFixture<SiteStartup> _site;

        public JsonApiSiteSetupTests()
        {
            var path = Path.Combine("src", "OrchardCore.Cms.Web");

            var appData = Path.Combine(EnvironmentHelpers.GetApplicationPath(), "App_Data", "Sites", "Tests");

            if (Directory.Exists(appData))
            {
                Directory.Delete(appData, true);
            }

            _site = new MvcTestFixture<SiteStartup>(path);
        }

        [Fact]
        public async Task ShouldSetSiteupUsingSqlite()
        {
            var siteName = Guid.NewGuid().ToString().Replace("-", "");

            string json = @"{
  ""data"": {
    ""type"": ""setup"",
    ""attributes"": {
        ""siteName"": """ + siteName + @""",
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
