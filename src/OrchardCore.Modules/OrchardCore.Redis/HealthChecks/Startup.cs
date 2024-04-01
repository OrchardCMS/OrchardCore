using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Redis.HealthChecks;

[RequireFeatures("OrchardCore.HealthChecks")]
public class Startup : StartupBase
{
    // The order of this startup configuration should be greater than zero to register the Redis check early,
    // so the health check can be reported alongside with other health checks in the system.
    public override int Order => 100;

    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddRedisCheck();
    }
}
