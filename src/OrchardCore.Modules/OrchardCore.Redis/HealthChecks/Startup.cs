using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Redis.HealthChecks;

[RequireFeatures("OrchardCore.HealthChecks")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddRedisCheck();
    }
}
