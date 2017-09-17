using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Tests.Apis.Sources;

namespace OrchardCore.Tests.Apis
{
    public class InMemorySiteContext : IDisposable
    {
        public MvcTestFixture<SiteStartup> Site { get; set; } 

        public InMemorySiteContext() {

            var path = Path.Combine("src", "OrchardCore.Cms.Web");

            var appData = Path.Combine(EnvironmentHelpers.GetApplicationPath(), "App_Data");

            if (Directory.Exists(appData))
            {
                Directory.Delete(appData, true);
            }

            Site = new MvcTestFixture<SiteStartup>(path);
        }

        public void Dispose()
        {
            Site.Dispose();
        }
    }
}
