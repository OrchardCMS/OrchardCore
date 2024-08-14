using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Workflows.Trimming.Drivers;
using OrchardCore.Workflows.Trimming.Services;

namespace OrchardCore.Workflows.Trimming;

[Feature("OrchardCore.Workflows")]
public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<IWorkflowTrimmingManager, WorkflowTrimmingManager>();
        services.AddSingleton<IBackgroundTask, WorkflowTrimmingBackgroundTask>();
        services.AddScoped<IDisplayDriver<ISite>, WorkflowTrimmingDisplayDriver>();
        services.AddScoped<INavigationProvider, AdminMenu>();

        services.Configure<WorkflowTrimmingOptions>(_shellConfiguration.GetSection("OrchardCore_Workflows").GetSection("Trimming"));
    }
}
