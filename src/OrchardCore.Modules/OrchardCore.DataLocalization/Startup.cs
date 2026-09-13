using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataLocalization.Deployment;
using OrchardCore.DataLocalization.Endpoints;
using OrchardCore.DataLocalization.Liquid;
using OrchardCore.DataLocalization.Recipes;
using OrchardCore.DataLocalization.Services;
using OrchardCore.Deployment;
using OrchardCore.Liquid;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.RemoteManagement;
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
        services.AddSingleton<IRemoteManagementCapabilityProvider, DataLocalizationRemoteManagementCapabilityProvider>();
        services.AddLiquidFilter<DataLocalizationFilter>("d");

        services.AddScoped<TranslationsManager>();
        services.AddScoped<ITranslationsManager>(provider => provider.GetRequiredService<TranslationsManager>());
        services.AddRecipeExecutionStep<TranslationsStep>();

        services.AddScoped<IDeploymentStepDefinition, TranslationsDeploymentStepDefinition>();
        services.AddDeployment<TranslationsDeploymentSource, TranslationsDeploymentStep, TranslationsDeploymentStepDriver>();
        services.AddDeployment<AllDataTranslationsDeploymentSource, AllDataTranslationsDeploymentStep, AllDataTranslationsDeploymentStepDriver>();
        services.AddSingleton<IDeploymentStepDefinition>(new EmptyDeploymentStepDefinition<AllDataTranslationsDeploymentStep>(nameof(AllDataTranslationsDeploymentStep)));

        services.AddScoped<IPermissionProvider, Permissions>();
        services.AddScoped<INavigationProvider, AdminMenu>();

        services.AddDataLocalization();
        services.AddSingleton<IDataTranslationProvider, DataTranslationProvider>();
    }

    /// <inheritdoc />
    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddTranslationManagementEndpoints();
    }
}
