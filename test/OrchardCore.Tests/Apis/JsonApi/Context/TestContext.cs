using System;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Tests.Apis.Sources;

namespace OrchardCore.Tests.Apis.JsonApi.Context
{
    public class TestContext : IDisposable
    {
        public OrchardTestFixture<SiteStartup> Site { get; }

        public OrchardGraphQLClient Client { get; }

        public TestContext()
        {
            Site = new OrchardTestFixture<SiteStartup>(EnvironmentHelpers.GetApplicationPath());

            Client = new OrchardGraphQLClient(Site.Client);
        }

        public void Dispose()
        {
            Site.Dispose();
        }
    }
}
