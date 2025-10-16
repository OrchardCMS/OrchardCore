using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Redis.Azure;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddKeyedTransient<ITokenProvider, AzureRedisTokenProvider>("Redis");
    }
}
