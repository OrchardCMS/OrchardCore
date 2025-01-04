using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries.Core;
using OrchardCore.Queries.Sql.Drivers;
using OrchardCore.Queries.Sql.Migrations;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql;

/// <summary>
/// These services are registered on the tenant service collection.
/// </summary>
[Feature("OrchardCore.Queries.Sql")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddDisplayDriver<Query, SqlQueryDisplayDriver>();
        services.AddQuerySource<SqlQuerySource>(SqlQuerySource.SourceName);

        services.AddNavigationProvider<AdminMenu>();
        services.AddDataMigration<SqlQueryMigrations>();
        services.AddScoped<IQueryHandler, SqlQueryHandler>();
    }
}
