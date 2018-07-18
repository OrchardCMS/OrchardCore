using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Apis.GraphQL.Client;
using Xunit;

namespace OrchardCore.Tests.Apis.GraphQL.Context
{
    public class TestContext // : IAsyncLifetime, IDisposable
    {
        public OrchardTestFixture<SiteStartup> Site { get; private set; }

        public OrchardGraphQLClient Client { get; private set; }

        public TestContext()
        {
            // Site = new OrchardTestFixture<SiteStartup>();
            // Site.ShellsContainerName = "Sites_" + GetType().FullName;
            // Client = new OrchardGraphQLClient(Site.CreateClient());
        }

        public void Initialize()
        {
            Site = new OrchardTestFixture<SiteStartup>();
            Site.ShellsContainerName = "Sites_" + GetType().FullName;
            var client = Site.CreateClient();
            client.Timeout = TimeSpan.FromMinutes(10);
            Client = new OrchardGraphQLClient(client);
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
