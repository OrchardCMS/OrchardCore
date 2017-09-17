using System;
using Microsoft.AspNetCore.Mvc.FunctionalTests;

namespace OrchardCore.Tests.Apis
{
    public class InMemorySiteContext : IDisposable
    {
        public MvcTestFixture<SiteStartup> Site { get; set; } = new MvcTestFixture<SiteStartup>();

        public void Dispose()
        {
            Site.Dispose();
        }
    }
}
