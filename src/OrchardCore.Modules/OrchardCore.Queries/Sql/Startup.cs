using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Queries.Sql.Drivers;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Queries.Sql
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [Feature("OrchardCore.Queries.Sql")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<Query>, SqlQueryDisplayDriver>();
            services.AddScoped<IQuerySource, SqlQuerySource>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }
    }
}
