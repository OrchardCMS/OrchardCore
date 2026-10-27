using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Roles.Workflows.Activities;
using OrchardCore.Roles.Workflows.Drivers;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Options;

namespace OrchardCore.Roles.Workflows;

[RequireFeatures("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WorkflowOptions>(options => options.CategoryIcons["User"] = "fa-solid fa-user");

        services.AddActivity<UnassignUserRoleTask, UnassignUserRoleTaskDisplayDriver>();
        services.AddActivity<GetUsersByRoleTask, GetUsersByRoleTaskDisplayDriver>();
    }
}
