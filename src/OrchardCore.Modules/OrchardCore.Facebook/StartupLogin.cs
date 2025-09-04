using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.Extensions.DependencyInjection;
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

namespace OrchardCore.Facebook;

[Feature(FacebookConstants.Features.Login)]
public sealed class StartupLogin : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddNavigationProvider<AdminMenuLogin>();

        services.AddSingleton<IFacebookLoginService, FacebookLoginService>();
        services.AddSiteDisplayDriver<FacebookLoginSettingsDisplayDriver>();
        services.AddRecipeExecutionStep<FacebookLoginSettingsStep>();

        // Register the options initializers required by the Facebook handler.
        // Orchard-specific initializers:
        services.AddTransient<IConfigureOptions<AuthenticationOptions>, FacebookLoginConfiguration>();
        services.AddTransient<IConfigureOptions<FacebookOptions>, FacebookLoginConfiguration>();

        // Built-in initializers:
        services.AddTransient<IPostConfigureOptions<FacebookOptions>, OAuthPostConfigureOptions<FacebookOptions, FacebookHandler>>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
public class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<FacebookLoginDeploymentSource, FacebookLoginDeploymentStep, FacebookLoginDeploymentStepDriver>();
    }
}
