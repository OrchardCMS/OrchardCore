using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Tests.Apis.Context
{
    public class OrchardTestFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public string ShellsContainerName { get; internal set; }
        public string ShellsContainerPath => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", ShellsContainerName);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (Directory.Exists(ShellsContainerPath))
            {
                Directory.Delete(ShellsContainerPath, true);
            }

            builder.UseContentRoot(Directory.GetCurrentDirectory());

            builder
                .ConfigureServices(services => {
                    services.AddSingleton(new TestSiteConfiguration { ShellsContainerName = ShellsContainerName });
                });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHostBuilderFactory.CreateFromAssemblyEntryPoint(
                typeof(Cms.Web.Startup).Assembly, Array.Empty<string>())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<SiteStartup>();
        }
    }

    public class TestSiteConfiguration
    {
        public string ShellsContainerName { get; set; }
    }
}