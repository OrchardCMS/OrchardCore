using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Orchard.Environment.Navigation;
using Orchard.Environment.Shell;
using Orchard.Recipes;
using Orchard.Roles.Recipes;
using Orchard.Roles.Services;
using Orchard.Security;
using Orchard.Security.Permissions;
using Orchard.Security.Services;

namespace Orchard.Roles
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddScoped<RoleManager<Role>>();
            services.TryAddScoped<IRoleStore<Role>, RoleStore>();
            services.TryAddScoped<IRoleProvider, RoleStore>();
            services.AddRecipeExecutionStep<RolesStep>();

            services.AddScoped<RoleUpdater>();
            services.AddScoped<IFeatureEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
            services.AddScoped<IAuthorizationHandler, RolesPermissionsHandler>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
        }

        public override void Configure(IApplicationBuilder builder, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }
    }
}
