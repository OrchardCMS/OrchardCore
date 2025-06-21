namespace OrchardCore.Tests.Apis.Context;

public class OrchardTestFixture<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    public string ContentRoot { get; } = Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var contentRoot = Path.Combine(Directory.GetCurrentDirectory(), ContentRoot);
        builder.UseContentRoot(contentRoot);
        builder.UseWebRoot(Path.Combine(contentRoot, "wwwroot"));
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
