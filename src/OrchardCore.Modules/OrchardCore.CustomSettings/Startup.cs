using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
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
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDeployment<CustomSettingsDeploymentSource, CustomSettingsDeploymentStep, CustomSettingsDeploymentStepDriver>();
        }
    }
}
