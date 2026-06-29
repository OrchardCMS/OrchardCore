using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DataOrchestrator.Activities;
using OrchardCore.DataOrchestrator.BackgroundTasks;
using OrchardCore.DataOrchestrator.Drivers;
using OrchardCore.DataOrchestrator.Exporting;
using OrchardCore.DataOrchestrator.Helpers;
using OrchardCore.DataOrchestrator.Indexes;
using OrchardCore.DataOrchestrator.Migrations;
using OrchardCore.DataOrchestrator.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataOrchestrator;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        // Core services.
        services.AddScoped<IEtlActivityLibrary, EtlActivityLibrary>();
        services.AddScoped<IEtlPipelineExecutor, EtlPipelineExecutor>();
        services.AddScoped<IEtlPipelineService, EtlPipelineService>();
        services.AddScoped<IEtlActivityDisplayManager, EtlActivityDisplayManager>();
        services.AddScoped<IEtlExportFormatProvider, EtlExportFormatProvider>();

        // Data persistence.
        services.AddDataMigration<DataOrchestratorIndexMigrations>();
        services.AddIndexProvider<EtlPipelineIndexProvider>();
        services.AddIndexProvider<EtlExecutionLogIndexProvider>();

        // Permissions and navigation.
        services.AddPermissionProvider<PermissionsProvider>();
        services.AddNavigationProvider<AdminMenu>();

        // Background task.
        services.AddSingleton<IBackgroundTask, EtlPipelineBackgroundTask>();

        // Resource management.
        services.AddResourceConfiguration<ResourceManagementOptionsConfiguration>();

        // Built-in export formats (shared by every file-based destination).
        services.AddEtlExportFormat<JsonExportFormat>();
        services.AddEtlExportFormat<CsvExportFormat>();
        services.AddEtlExportFormat<ExcelExportFormat>();

        // Built-in source activities.
        services.AddEtlActivity<ContentItemSource, ContentItemSourceDisplayDriver>();
        services.AddEtlActivity<ExcelSource, ExcelSourceDisplayDriver>();
        services.AddEtlActivity<JsonSource, JsonSourceDisplayDriver>();
        services.AddEtlActivity<QuerySource, QuerySourceDisplayDriver>();

        // Built-in transform activities.
        services.AddEtlActivity<FieldMappingTransform, FieldMappingTransformDisplayDriver>();
        services.AddEtlActivity<FilterTransform, FilterTransformDisplayDriver>();
        services.AddEtlActivity<FormatValueTransform, FormatValueTransformDisplayDriver>();
        services.AddEtlActivity<JoinDataSetsTransform, JoinDataSetsTransformDisplayDriver>();

        // Built-in load activities (destinations).
        services.AddEtlActivity<MediaFolderExportLoad, MediaFolderExportLoadDisplayDriver>();
        services.AddEtlActivity<FtpExportLoad, FtpExportLoadDisplayDriver>();
        services.AddEtlActivity<ContentItemLoad, ContentItemLoadDisplayDriver>();
    }
}
