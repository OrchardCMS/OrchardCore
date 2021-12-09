using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Facebook.Deployment;
using OrchardCore.Facebook.Login.Configuration;
using OrchardCore.Facebook.Login.Drivers;
using OrchardCore.Facebook.Login.Recipes;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Recipes.Services;
using OrchardCore.Settings;

namespace OrchardCore.Facebook
{
    [Feature(FacebookConstants.Features.Login)]
    public class StartupLogin : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INavigationProvider, AdminMenuLogin>();

            services.AddSingleton<IFacebookLoginService, FacebookLoginService>();
            services.AddScoped<IDisplayDriver<ISite>, FacebookLoginSettingsDisplayDriver>();
            services.AddRecipeExecutionStep<FacebookLoginSettingsStep>();

            // Register the options initializers required by the Facebook handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, FacebookLoginConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<FacebookOptions>, FacebookLoginConfiguration>(),

                // Deployment

                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<FacebookOptions>, OAuthPostConfigureOptions<FacebookOptions, FacebookHandler>>()
            });
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<DeploymentStep>, FacebookLoginDeploymentStepDriver>();
            services.AddTransient<IDeploymentSource, FacebookLoginDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory, DeploymentStepFactory<FacebookLoginDeploymentStep>>();
        }
    }
}
