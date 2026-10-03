using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Roles.Workflows.Activities;
using OrchardCore.Roles.Workflows.Drivers;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Roles.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<UnassignUserRoleTask, UnassignUserRoleTaskDisplayDriver>();
        services.AddActivity<GetUsersByRoleTask, GetUsersByRoleTaskDisplayDriver>();
    }
}
