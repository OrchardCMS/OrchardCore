using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Testing.Context
{
    public class OrchardTestFixture : WebApplicationFactory<SiteStartup>
    {
        public static Action<IHostBuilder> ConfigureHostBuilder;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var shellsApplicationDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");

            if (Directory.Exists(shellsApplicationDataPath))
            {
                Directory.Delete(shellsApplicationDataPath, true);
            }

            builder.UseContentRoot(Directory.GetCurrentDirectory());
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHostBuilderFactory.CreateFromAssemblyEntryPoint(
                SiteStartup.WebStartupClass.Assembly, Array.Empty<string>());
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            var builder = Host.CreateDefaultBuilder()
                  .ConfigureWebHostDefaults(webBuilder =>
                      webBuilder.UseStartup<SiteStartup>());

            if (ConfigureHostBuilder != null)
            {
                ConfigureHostBuilder(builder);
            }

            return builder;
        }
    }
}
