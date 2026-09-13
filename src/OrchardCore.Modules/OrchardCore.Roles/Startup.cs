using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Extensions;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;
using OrchardCore.RemoteManagement;
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
using OrchardCore.Roles.Endpoints.Management;

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
        services.AddSingleton<IRemoteManagementCapabilityProvider, RoleRemoteManagementCapabilityProvider>();
        services.AddScoped<IUserClaimsProvider, RoleClaimsProvider>();

        services.AddDataMigration<SystemRolesMigrations>();
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

        services.AddScoped(static serviceProvider =>
        {
            var permissionProviders = serviceProvider.GetServices<IPermissionProvider>();
            var typeFeatureProvider = serviceProvider.GetRequiredService<ITypeFeatureProvider>();
            var shellFeaturesManager = serviceProvider.GetRequiredService<IShellFeaturesManager>();
            return CreatePermissionCatalogAsync(permissionProviders, typeFeatureProvider, shellFeaturesManager).GetAwaiter().GetResult();
        });
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddRoleManagementEndpoints();
    }

    private static async Task<PermissionCatalog> CreatePermissionCatalogAsync(IEnumerable<IPermissionProvider> permissionProviders, ITypeFeatureProvider typeFeatureProvider, IShellFeaturesManager shellFeaturesManager)
    {
        var enabledFeatures = await shellFeaturesManager.GetEnabledFeaturesAsync();
        var permissions = new Dictionary<string, Permission>(StringComparer.Ordinal);
        var featureIds = new Dictionary<string, string>(StringComparer.Ordinal);
        var featureNames = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var provider in permissionProviders)
        {
            var feature = typeFeatureProvider.GetFeaturesForDependency(provider.GetType()).LastOrDefault(candidate => enabledFeatures.Any(enabled => enabled.Id == candidate.Id));
            foreach (var permission in await provider.GetPermissionsAsync())
            {
                permissions.TryAdd(permission.Name, permission);
                if (feature != null)
                {
                    featureIds.TryAdd(permission.Name, feature.Id);
                    featureNames.TryAdd(permission.Name, string.IsNullOrWhiteSpace(feature.Name) ? feature.Id : feature.Name);
                }
            }
        }

        return new PermissionCatalog(permissions.Values, featureIds, featureNames);
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AllRolesDeploymentSource, AllRolesDeploymentStep, AllRolesDeploymentStepDriver>();
        services.AddSingleton<IDeploymentStepDefinition>(new EmptyDeploymentStepDefinition<AllRolesDeploymentStep>(nameof(AllRolesDeploymentStep)));
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

[RequireFeatures("OrchardCore.DataLocalization")]
public sealed class DataLocalizationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ILocalizationDataProvider, PermissionsLocalizationDataProvider>();
    }
}
