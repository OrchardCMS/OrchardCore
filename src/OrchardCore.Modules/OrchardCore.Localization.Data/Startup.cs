using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Localization.Data.Deployment;
using OrchardCore.Localization.Data.Recipes;
using OrchardCore.Localization.Data.Services;
using OrchardCore.Modules;
using OrchardCore.Recipes;

namespace OrchardCore.Localization.Data
{
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

            services.AddTransient<IDeploymentSource, AllDataTranslationsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllDataTranslationsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllDataTranslationsDeploymentStepDriver>();

            services.AddScoped<ILocalizationDataProvider, ContentTypeDataLocalizationProvider>();
            services.AddScoped<ILocalizationDataProvider, ContentFieldDataLocalizationProvider>();

            services.AddDataLocalization();
        }
    }
}
