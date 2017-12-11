using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Tests.Apis.Sources;
using Xunit;

namespace OrchardCore.Tests.Apis.Json
{
    public class JsonSiteSetupTests : IDisposable
    {
        private OrchardTestFixture<SiteStartup> _site;

        public JsonSiteSetupTests()
        {
            _site = new OrchardTestFixture<SiteStartup>(EnvironmentHelpers.GetApplicationPath());
        }

        [Fact]
        public async Task ShouldSetSiteupUsingSqlite()
        {
            string json = @"{
        ""siteName"": """ + _site.SiteName + @""",
        ""databaseProvider"": ""Sqlite"",
        ""userName"": ""admin"",
        ""email"": ""fu@bar.com"",
        ""password"": ""Password01_"",
        ""passwordConfirmation"": ""Password01_"",
        ""recipeName"": ""Blog""
}";

            var response = await _site.Client.PostJsonAsync("/api/site", json);

            var value = await response.Content.ReadAsStringAsync();

            //Assert.Equal(32, JObject.Parse(value)["data"]["createSite"]["executionId"].Value<string>().Length);
        }

        public void Dispose()
        {
            _site.Dispose();
        }
    }
}
