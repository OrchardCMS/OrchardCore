using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Tests.Apis.Context
{
    public class OrchardTestFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
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
                typeof(Program).Assembly, Array.Empty<string>());
        }

        protected override IHostBuilder CreateHostBuilder()
            => Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.UseStartup<TStartup>());
    }
}
