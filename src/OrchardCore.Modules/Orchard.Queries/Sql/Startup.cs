using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Handlers;
using Orchard.Environment.Navigation;
using Orchard.Queries.Sql.Drivers;
using Orchard.Security.Permissions;

namespace Orchard.Queries.Sql
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    [Feature("Orchard.Queries.Sql")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<Query>, SqlQueryDisplayDriver>();
            services.AddScoped<IQuerySource, SqlQuerySource>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
