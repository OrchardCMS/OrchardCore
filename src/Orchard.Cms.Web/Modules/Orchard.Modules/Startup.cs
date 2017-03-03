using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Navigation;
using Orchard.Modules.Recipes.Executors;
using Orchard.Modules.Services;
using Orchard.Recipes;
using Orchard.Security.Permissions;

namespace Orchard.Modules
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddRecipeExecutionStep<FeatureStep>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IModuleService, ModuleService>();
            services.AddScoped<INavigationProvider, AdminMenu>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
