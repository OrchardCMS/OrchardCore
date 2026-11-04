using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Deployment;
using OrchardCore.CustomSettings.Drivers;
using OrchardCore.CustomSettings.Endpoints;
using OrchardCore.CustomSettings.Recipes;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.RemoteManagement;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<CustomSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<CustomSettingsService>();
        services.AddScoped<CustomSettingsManagementService>();
        services.AddScoped<ISiteSettingsManagementSchemaProvider, CustomSettingsManagementSchemaProvider>();
        services.AddScoped<IStereotypesProvider, CustomSettingsStereotypesProvider>();
        // Permissions
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IAuthorizationHandler, CustomSettingsAuthorizationHandler>();
        services.AddSingleton<IRemoteManagementCapabilityProvider, CustomSettingsRemoteManagementCapabilityProvider>();

        services.AddRecipeExecutionStep<CustomSettingsStep>();

        services.Configure<ContentTypeDefinitionOptions>(options =>
        {
            options.Stereotypes.TryAdd("CustomSettings", new ContentTypeDefinitionDriverOptions
            {
                ShowCreatable = false,
                ShowListable = false,
                ShowDraftable = false,
                ShowVersionable = false,
            });
        });
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddCustomSettingsManagementEndpoints();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<CustomSettingsDeploymentSource, CustomSettingsDeploymentStep, CustomSettingsDeploymentStepDriver>();
        services.AddScoped<IDeploymentStepDefinition>(provider => new NamedSelectionDeploymentStepDefinition<CustomSettingsDeploymentStep>(
            nameof(CustomSettingsDeploymentStep), "settingsTypeNames",
            () => provider.GetRequiredService<CustomSettingsService>().GetAllSettingsTypeNamesAsync(),
            step => (step.IncludeAll, step.SettingsTypeNames),
            (step, includeAll, names) => { step.IncludeAll = includeAll; step.SettingsTypeNames = names; }));
    }
}
