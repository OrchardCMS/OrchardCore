using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.DataPipelines.Services;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Contents;
using OrchardCore.DataSources.Queries;
using OrchardCore.DataSources.Users;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataPipelines.Startups;

/// <summary>
/// The reusable layer of the <c>OrchardCore.DataPipelines.Core</c> feature: data sources, file formats, the formula
/// language and its functions, data transforms, and shared links, used by data pipelines and other modules, such as
/// reports.
/// </summary>
[Feature(DataPipelinesConstants.CoreFeatureId)]
public sealed class CoreStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSourcesCore();
        services.AddScoped<IDataTimeZoneResolver, DataTimeZoneResolver>();

        // Shared links: any module shares a kind of resource by registering a handler (AddSharedLinkHandler).
        services.AddDataMigration<SharedLinkMigrations>();
        services.AddIndexProvider<SharedLinkIndexProvider>();
        services.AddScoped<SharedLinkUseCounter>();
        services.AddScoped<SharedLinkManager>();
        services.AddScoped<ISharedLinkManager>(serviceProvider => serviceProvider.GetRequiredService<SharedLinkManager>());
        services.AddPermissionProvider<SharedLinkPermissionsProvider>();
        services.AddNavigationProvider<SharedLinksAdminMenu>();
        services.AddSingleton<IBackgroundTask, SharedLinksBackgroundTask>();
    }
}

[Feature(DataPipelinesConstants.CoreFeatureId)]
[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentsDataSourceStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<ContentDataSourceOptions>();
        services.AddDataSource<ContentItemsDataSource>();
    }
}

[Feature(DataPipelinesConstants.CoreFeatureId)]
[RequireFeatures("OrchardCore.Users")]
public sealed class UsersDataSourceStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<UsersDataSource>();
    }
}

[Feature(DataPipelinesConstants.CoreFeatureId)]
[RequireFeatures("OrchardCore.Queries")]
public sealed class QueriesDataSourceStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<QueriesDataSource>();
    }
}
