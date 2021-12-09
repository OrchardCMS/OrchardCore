using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
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
using OrchardCore.Settings;

namespace OrchardCore.CustomSettings
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IDisplayDriver<ISite>, CustomSettingsDisplayDriver>();
            services.AddScoped<CustomSettingsService>();

            // Permissions
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IAuthorizationHandler, CustomSettingsAuthorizationHandler>();

            services.AddRecipeExecutionStep<CustomSettingsStep>();
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, CustomSettingsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<CustomSettingsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, CustomSettingsDeploymentStepDriver>();
        }
    }
}
