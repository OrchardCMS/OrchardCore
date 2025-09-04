namespace OrchardCore.Tests.Apis.Context;

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
            typeof(Program).Assembly, []);
    }

    protected override IHostBuilder CreateHostBuilder()
        => Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
                webBuilder.UseStartup<TStartup>());
}
