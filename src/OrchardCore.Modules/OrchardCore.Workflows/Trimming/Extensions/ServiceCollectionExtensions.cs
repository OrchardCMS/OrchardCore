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
        services.AddJob<WorkflowTrimmingJob>(WorkflowTrimmingJob.Name, job =>
        {
            job.Title = OrchardCore.Workflows.WorkflowsLocalizationSources.TrimmingJobTitle;
            job.Description = OrchardCore.Workflows.WorkflowsLocalizationSources.TrimmingJobDescription;
            job.Schedule = "0 0 * * *";
        });
        services.AddSiteDisplayDriver<WorkflowTrimmingDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        // The 'OrchardCore_Workflows' section is deprecated and will be removed in a future major version, use 'Workflows' instead.
        services.Configure<WorkflowTrimmingOptions>(shellConfiguration.GetSectionCompat("Workflows:Trimming", "OrchardCore_Workflows:Trimming"));

        return services;
    }
}
