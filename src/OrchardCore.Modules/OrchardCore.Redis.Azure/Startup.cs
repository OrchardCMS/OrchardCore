using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Redis.Azure;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IRedisTokenProvider, AzureRedisTokenProvider>();
        services.Configure<AzureRedisOptions>(_shellConfiguration.GetSection("OrchardCore_Redis"));
    }
}
