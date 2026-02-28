using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Email.Azure.HealthChecks;

[RequireFeatures("OrchardCore.HealthChecks")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddAzureEmailCheck();
    }
}
