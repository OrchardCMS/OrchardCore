using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Tenants.Workflows.Activities;
using OrchardCore.Tenants.Workflows.Drivers;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Tenants.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<DisableTenantTask, DisableTenantTaskDisplayDriver>();
        services.AddActivity<EnableTenantTask, EnableTenantTaskDisplayDriver>();
        services.AddActivity<CreateTenantTask, CreateTenantTaskDisplayDriver>();
        services.AddActivity<SetupTenantTask, SetupTenantTaskDisplayDriver>();
    }
}
