using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataSources;
using OrchardCore.DataSources.Contents;
using OrchardCore.DataSources.Queries;
using OrchardCore.DataSources.Users;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Startups;

/// <summary>
/// The reusable data layer of the <c>OrchardCore.DataPipelines.Core</c> feature: data sources, file formats, the
/// formula language and its functions, and data transforms, shared by data pipelines and other modules, such as reports.
/// </summary>
[Feature(DataPipelinesConstants.CoreFeatureId)]
public sealed class CoreStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSourcesCore();
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
