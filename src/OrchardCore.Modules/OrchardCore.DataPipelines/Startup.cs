using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DataPipelines.BackgroundTasks;
using OrchardCore.DataPipelines.Drivers;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Migrations;
using OrchardCore.DataPipelines.Recipes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell.Configuration;
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
        services.AddIndexProvider<DataPipelineSharedFileIndexProvider>();

        services.AddScoped<DataPipelineManager>();
        services.AddScoped<DataPipelineRunManager>();
        services.AddScoped<DataPipelineRunExecutor>();
        services.AddScoped<DataPipelineUserResolver>();
        services.AddScoped<DataPipelineEditorContext>();
        services.AddScoped<DataPipelineDesignerModelBuilder>();
        services.AddSingleton<DataPipelineSecrets>();
        services.AddScoped<DataPipelineSharedFileManager>();
        services.AddScoped<IDataPipelineSharedFileManager>(serviceProvider => serviceProvider.GetRequiredService<DataPipelineSharedFileManager>());
        services.AddSingleton<DataPipelineRunTracker>();
        services.AddSingleton<DataPipelineRunDispatcher>();
        services.AddSingleton<IBackgroundTask, DataPipelineRunsBackgroundTask>();

        services.AddScoped<IDisplayManager<DataPipelineStep>, DisplayManager<DataPipelineStep>>();
        services.AddDisplayDriver<DataPipelineStep, DataPipelineStepTitleDisplayDriver>();

        services.AddPermissionProvider<PermissionsProvider>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        services.AddDataPipelineStep<DataSourceStep, DataSourceStepDisplayDriver>();
        services.AddDataPipelineStep<FilterStep, FilterStepDisplayDriver>();
        services.AddDataPipelineStep<CalculatedFieldsStep, CalculatedFieldsStepDisplayDriver>();
        services.AddDataPipelineStep<SelectFieldsStep, SelectFieldsStepDisplayDriver>();
        services.AddDataPipelineStep<SortStep, SortStepDisplayDriver>();
        services.AddDataPipelineStep<AggregateStep, AggregateStepDisplayDriver>();
        services.AddDataPipelineStep<JoinStep, JoinStepDisplayDriver>();
        services.AddDataPipelineStep<UnionStep, UnionStepDisplayDriver>();
        services.AddDataPipelineStep<DistinctStep, DistinctStepDisplayDriver>();
        services.AddDataPipelineStep<LimitStep, LimitStepDisplayDriver>();
        services.AddDataPipelineStep<CreateFileStep, CreateFileStepDisplayDriver>();
        services.AddDataPipelineStep<ZipFilesStep, ZipFilesStepDisplayDriver>();
        services.AddDataPipelineStep<ShareDownloadLinkStep, ShareDownloadLinkStepDisplayDriver>();

        services.AddRecipeExecutionStep<DataPipelinesRecipeStep>();
    }
}
