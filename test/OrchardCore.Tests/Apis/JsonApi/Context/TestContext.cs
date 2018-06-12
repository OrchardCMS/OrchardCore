using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.FunctionalTests;
using OrchardCore.Apis.JsonApi.Client;
using OrchardCore.Tests.Apis.Sources;
using Xunit;

namespace OrchardCore.Tests.Apis.JsonApi.Context
{
    public class TestContext : IAsyncLifetime, IDisposable
    {
        public OrchardTestFixture<SiteStartup> Site { get; }

        public OrchardJsonApiClient Client { get; }

        public TestContext()
        {
            Site = new OrchardTestFixture<SiteStartup>(EnvironmentHelpers.GetApplicationPath());

            Client = new OrchardJsonApiClient(Site.CreateClient());
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
