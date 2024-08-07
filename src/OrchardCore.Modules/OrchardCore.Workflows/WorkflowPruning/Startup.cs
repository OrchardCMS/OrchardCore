using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Workflows.WorkflowPruning.Drivers;
using OrchardCore.Workflows.WorkflowPruning.Services;

namespace OrchardCore.Workflows.WorkflowPruning;

[Feature("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IWorkflowPruningManager, WorkflowPruningManager>();
        services.AddSingleton<IBackgroundTask, WorkflowPruningBackgroundTask>();
        services.AddScoped<IDisplayDriver<ISite>, WorkflowPruningDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();
    }
}
