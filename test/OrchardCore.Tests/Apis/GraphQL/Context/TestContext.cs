using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using Microsoft.AspNetCore.Mvc.Testing;
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
            Site.ShellsContainerName = "Sites_" + GetType().FullName;

            var options = new WebApplicationFactoryClientOptions();
            var builder = new UriBuilder(options.BaseAddress);
            builder.Port = 5000 + new Random().Next(100);
            options.BaseAddress = builder.Uri;

            Client = new OrchardGraphQLClient(Site.CreateClient(options));
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
