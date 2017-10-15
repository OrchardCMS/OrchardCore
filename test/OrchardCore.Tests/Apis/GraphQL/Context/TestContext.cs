using System;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.GraphQL.Client;
using OrchardCore.Tests.Apis.Sources;

namespace OrchardCore.Tests.Apis.GraphQL.Context
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
