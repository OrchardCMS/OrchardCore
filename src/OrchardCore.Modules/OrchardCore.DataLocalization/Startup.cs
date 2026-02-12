using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataLocalization.Deployment;
using OrchardCore.DataLocalization.Recipes;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataLocalization;

/// <summary>
/// Represents a localization module entry point.
/// </summary>
public class Startup : StartupBase
{
    public override int ConfigureOrder => -100;

    /// <inheritdocs />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<TranslationsManager>();
        services.AddRecipeExecutionStep<TranslationsStep>();

        // Legacy deployment step (kept for backward compatibility).
        services.AddTransient<IDeploymentSource, AllDataTranslationsDeploymentSource>();
        services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllDataTranslationsDeploymentStep>());
        services.AddScoped<IDisplayDriver<DeploymentStep>, AllDataTranslationsDeploymentStepDriver>();

        // New deployment step with culture/category filtering.
        services.AddDeployment<TranslationsDeploymentSource, TranslationsDeploymentStep, TranslationsDeploymentStepDriver>();

        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();

        services.AddDataLocalization();
        services.AddSingleton<IDataTranslationProvider, DataTranslationProvider>();
    }
}
