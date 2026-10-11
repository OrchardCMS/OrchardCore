using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataSources.Contents;
using OrchardCore.DataSources.Queries;
using OrchardCore.DataSources.Users;
using OrchardCore.Modules;

namespace OrchardCore.DataSources;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSourcesCore();
    }
}

[RequireFeatures("OrchardCore.Contents")]
public sealed class ContentsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<ContentDataSourceOptions>();
        services.AddDataSource<ContentItemsDataSource>();
    }
}

[RequireFeatures("OrchardCore.Users")]
public sealed class UsersStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<UsersDataSource>();
    }
}

[RequireFeatures("OrchardCore.Queries")]
public sealed class QueriesStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDataSource<QueriesDataSource>();
    }
}
