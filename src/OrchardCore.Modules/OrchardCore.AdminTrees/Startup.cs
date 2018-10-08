using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.AdminTrees.Indexes;
using OrchardCore.AdminTrees.Services;
using OrchardCore.AdminTrees.AdminNodes;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using YesSql.Indexes;

namespace OrchardCore.AdminTrees
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddSingleton<IIndexProvider, AdminTreeIndexProvider>();
            services.AddTransient<IDataMigration, Migrations>();

            services.AddScoped<AdminTreeNavigationProvidersCoordinator, AdminTreeNavigationProvidersCoordinator>();

            services.AddScoped<IDisplayManager<MenuItem>, DisplayManager<MenuItem>>();

            // link treeNode
            services.AddSingleton<IAdminNodeProviderFactory>(new AdminNodeProviderFactory<LinkAdminNode>());
            services.AddScoped<IAdminNodeNavigationBuilder, LinkAdminNodeNavigationBuilder>();
            services.AddScoped<IDisplayDriver<MenuItem>, LinkAdminNodeDriver>();

        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}