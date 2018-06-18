using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Apis.GraphQL.Client;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Context
{
    public class TestContext : IAsyncLifetime, IDisposable
    {
        public OrchardTestFixture<SiteStartup> Site { get; }

        public OrchardGraphQLClient Client { get; }

        public TestContext()
        {
            Site = new OrchardTestFixture<SiteStartup>();

            Client = new OrchardGraphQLClient(Site.CreateClient());
        }

        public void Dispose()
        {
            Site.Dispose();
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
