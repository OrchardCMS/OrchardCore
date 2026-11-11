using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Jobs;
using OrchardCore.DataPipelines.Migrations;
using OrchardCore.DataPipelines.Recipes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.ResourceManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataPipelines;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataPipelinesCore();
        services.Configure<DataPipelineOptions>(_shellConfiguration.GetSection("DataPipelines"));

        services.AddDataMigration<DataPipelineMigrations>();
        services.AddIndexProvider<DataPipelineIndexProvider>();
        services.AddIndexProvider<DataPipelineVersionIndexProvider>();
        services.AddIndexProvider<DataPipelineRunIndexProvider>();
        services.AddIndexProvider<DataPipelineStateIndexProvider>();

        services.AddScoped<DataPipelineManager>();
        services.AddScoped<DataPipelineRunManager>();
        services.AddScoped<DataPipelineStateManager>();
        services.AddScoped<DataPipelineRunExecutor>();
        services.AddScoped<DataPipelineUserResolver>();
        services.AddScoped<DataPipelineEditorContext>();
        services.AddScoped<DataPipelineDesignerModelBuilder>();
        services.AddSingleton<DataPipelineSecrets>();
        services.AddScoped<DataPipelineFileLinkHandler>();
        services.AddSharedLinkHandler<DataPipelineFileLinkHandler>();

        // A run of a pipeline executes as a job run, which a single server takes, whatever their number.
        services.AddJob<DataPipelineRunJob>(DataPipelineRunJob.Name, job =>
        {
            job.Title = DataPipelinesLocalizationSources.RunJobTitle;
            job.Description = DataPipelinesLocalizationSources.RunJobDescription;
        });
        services.AddScoped<IJobRunHandler, DataPipelineJobRunHandler>();
        services.AddJob<DataPipelineRunsCleanupJob>(DataPipelineRunsCleanupJob.Name, job =>
        {
            job.Title = DataPipelinesLocalizationSources.RunsCleanupJobTitle;
            job.Description = DataPipelinesLocalizationSources.RunsCleanupJobDescription;
            job.Schedule = "0 * * * *";

            // It runs often, and only its failures are worth keeping.
            job.KeepSucceededRuns = false;
        });

        services.AddScoped<IDisplayManager<DataPipelineStep>, DisplayManager<DataPipelineStep>>();
        services.AddDisplayDriver<DataPipelineStep, DataPipelineStepTitleDisplayDriver>();

        services.AddPermissionProvider<PermissionsProvider>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        services.AddDataPipelineStep<DataSourceStep, DataSourceStepDisplayDriver>();
        services.AddDataPipelineStep<FilterStep, FilterStepDisplayDriver>();
        services.AddDataPipelineStep<CalculatedFieldsStep, CalculatedFieldsStepDisplayDriver>();
        services.AddDataPipelineStep<ConvertTimeZoneStep, ConvertTimeZoneStepDisplayDriver>();
        services.AddDataPipelineStep<SelectFieldsStep, SelectFieldsStepDisplayDriver>();
        services.AddDataPipelineStep<SortStep, SortStepDisplayDriver>();
        services.AddDataPipelineStep<AggregateStep, AggregateStepDisplayDriver>();
        services.AddDataPipelineStep<JoinStep, JoinStepDisplayDriver>();
        services.AddDataPipelineStep<UnionStep, UnionStepDisplayDriver>();
        services.AddDataPipelineStep<DistinctStep, DistinctStepDisplayDriver>();
        services.AddDataPipelineStep<LimitStep, LimitStepDisplayDriver>();
        services.AddDataPipelineStep<CreateFileStep, CreateFileStepDisplayDriver>();
        services.AddDataPipelineStep<ZipFilesStep, ZipFilesStepDisplayDriver>();

        services.AddRecipeExecutionStep<DataPipelinesRecipeStep>();
    }
}
