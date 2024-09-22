using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Deployment;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Roles.Core;
using OrchardCore.Roles.Deployment;
using OrchardCore.Roles.Recipes;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IAuthorizationHandler, RolesPermissionHandler>();
        services.AddScoped<IRoleTracker, RoleTracker>();
        services.AddScoped<RoleStore>();
        services.Replace(ServiceDescriptor.Scoped<IRoleClaimStore<IRole>>(sp => sp.GetRequiredService<RoleStore>()));
        services.Replace(ServiceDescriptor.Scoped<IRoleStore<IRole>>(sp => sp.GetRequiredService<RoleStore>()));

        services.AddRecipeExecutionStep<RolesStep>();
        services.AddScoped<IAuthorizationHandler, RolesPermissionsHandler>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllRolesDeploymentSource, AllRolesDeploymentStep, AllRolesDeploymentStepDriver>();
    }
}

[Feature("OrchardCore.Roles.Core")]
public sealed class RoleUpdaterStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<RoleManager<IRole>>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<RoleUpdater>();
        services.AddScoped<IFeatureEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
        services.AddScoped<IRoleCreatedEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
        services.AddScoped<IRoleRemovedEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
    }
}
