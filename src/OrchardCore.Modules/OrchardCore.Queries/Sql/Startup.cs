using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Queries.Sql.Drivers;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql
{
    /// <summary>
    /// These services are registered on the tenant service collection.
    /// </summary>
    [Feature("OrchardCore.Queries.Sql")]
    public sealed class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<Query>, SqlQueryDisplayDriver>();
            services.AddScoped<IQuerySource, SqlQuerySource>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            // Allows to serialize 'SqlQuery' from its base type.
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddJsonDerivedTypeInfo<SqlQuery, Query>();
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
