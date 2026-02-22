using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.CustomSettings.Deployment;
using OrchardCore.CustomSettings.Drivers;
using OrchardCore.CustomSettings.Recipes;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.CustomSettings;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<CustomSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddScoped<CustomSettingsService>();
        services.AddScoped<IStereotypesProvider, CustomSettingsStereotypesProvider>();
        // Permissions
        services.AddPermissionProvider<Permissions>();
        services.AddScoped<IAuthorizationHandler, CustomSettingsAuthorizationHandler>();

		#pragma warning disable CS0618 // Type or member is obsolete
		services.AddRecipeExecutionStep<CustomSettingsStep>();
		#pragma warning restore CS0618 // Type or member is obsolete
		services.AddRecipeDeploymentStep<CustomSettingsRecipeStep>();

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
}

[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<CustomSettingsDeploymentSource, CustomSettingsDeploymentStep, CustomSettingsDeploymentStepDriver>();
    }
}
