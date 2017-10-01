using System;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Newtonsoft.Json;
using OrchardCore.Tests.Apis.Sources;

namespace OrchardCore.Tests.Apis.GraphQL
{
    public class BlogSiteContext : IDisposable
    {
        public OrchardTestFixture<SiteStartup> Site { get; }

        public BlogSiteContext()
        {
            Site = new OrchardTestFixture<SiteStartup>(EnvironmentHelpers.GetApplicationPath());

            var variables =
@"{ 
    ""site"": {
        ""siteName"": """ + Site.SiteName + @""",
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

            var response = Site.Client.PostJsonAsync("graphql", json).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();
        }

        public void Dispose()
        {
            Site.Dispose();
        }
    }
}
