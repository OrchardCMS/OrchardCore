using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Testing.Infrastructure;

public class OrchardCoreTestFixture<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
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
        => WebHostBuilderFactory.CreateFromAssemblyEntryPoint(typeof(TStartup).Assembly, Array.Empty<string>());

    protected override IHostBuilder CreateHostBuilder()
        => Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TStartup>());
}
