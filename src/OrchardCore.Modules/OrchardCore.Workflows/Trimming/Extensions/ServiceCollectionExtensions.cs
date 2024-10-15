using OrchardCore.BackgroundTasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Navigation;
using OrchardCore.Workflows.Trimming;
using OrchardCore.Workflows.Trimming.Drivers;
using OrchardCore.Workflows.Trimming.Services;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrimmingServices(this IServiceCollection services, IShellConfiguration shellConfiguration)
    {
        services.AddScoped<IWorkflowTrimmingService, WorkflowTrimmingService>();
        services.AddSingleton<IBackgroundTask, WorkflowTrimmingBackgroundTask>();
        services.AddSiteDisplayDriver<WorkflowTrimmingDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.Configure<WorkflowTrimmingOptions>(shellConfiguration.GetSection("OrchardCore_Workflows:Trimming"));

        return services;
    }
}
