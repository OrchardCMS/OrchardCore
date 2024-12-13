using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Roles.Deployment;
using OrchardCore.Roles.Migrations;
using OrchardCore.Roles.Recipes;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;
using OrchardCore.Users.Services;

namespace OrchardCore.Roles;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IUserClaimsProvider, RoleClaimsProvider>();
        services.AddDataMigration<RolesMigrations>();
        services.AddScoped<RoleStore>();
        services.Replace(ServiceDescriptor.Scoped<IRoleClaimStore<IRole>>(sp => sp.GetRequiredService<RoleStore>()));
        services.Replace(ServiceDescriptor.Scoped<IRoleStore<IRole>>(sp => sp.GetRequiredService<RoleStore>()));
        services.AddRecipeExecutionStep<RolesStep>();
        services.AddScoped<IAuthorizationHandler, RolesPermissionsHandler>();
        services.AddPermissionProvider<Permissions>();
        services.AddNavigationProvider<AdminMenu>();
        services.Configure<SystemRoleOptions>(options =>
        {
            var adminRoleName = _shellConfiguration.GetSection("OrchardCore_Roles").GetValue<string>("AdminRoleName");

            if (!string.IsNullOrWhiteSpace(adminRoleName))
            {
                options.SystemAdminRoleName = adminRoleName;
            }
            else
            {
                options.SystemAdminRoleName = OrchardCoreConstants.Roles.Administrator;
            }
        });
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
        services.AddRolesCoreServices();
        services.AddScoped<RoleManager<IRole>>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<RoleUpdater>();
        services.AddScoped<IFeatureEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
        services.AddScoped<IRoleCreatedEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
        services.AddScoped<IRoleRemovedEventHandler>(sp => sp.GetRequiredService<RoleUpdater>());
    }
}
