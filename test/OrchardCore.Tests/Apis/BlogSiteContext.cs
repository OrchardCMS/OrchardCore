using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Newtonsoft.Json;
using OrchardCore.Tests.Apis.Sources;

namespace OrchardCore.Tests.Apis
{
    public class BlogSiteContext : IDisposable
    {
        public MvcTestFixture<SiteStartup> Site { get; }

        public BlogSiteContext()
        { 
            var path = Path.Combine("src", "OrchardCore.Cms.Web");

            var appData = Path.Combine(EnvironmentHelpers.GetApplicationPath(), "App_Data", "Sites", "Tests");

            if (Directory.Exists(appData))
            {
                Directory.Delete(appData, true);
            }

            var siteName = Guid.NewGuid().ToString().Replace("-", "");

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

            var site = new MvcTestFixture<SiteStartup>(path);

            var response = site.Client.PostJsonAsync("graphql", json).GetAwaiter().GetResult();

            response.EnsureSuccessStatusCode();

            Site = site;
        }

        public void Dispose()
        {
            Site.Dispose();
        }
    }
}
