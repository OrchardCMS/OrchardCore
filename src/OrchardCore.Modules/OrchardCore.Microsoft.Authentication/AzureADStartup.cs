using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Deployment;
using OrchardCore.Microsoft.Authentication.Drivers;
using OrchardCore.Microsoft.Authentication.Recipes;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Microsoft.Authentication;

[Feature(MicrosoftAuthenticationConstants.Features.AAD)]
public sealed class AzureADStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();

        services.AddSingleton<IAzureADService, AzureADService>();
        services.AddRecipeExecutionStep<AzureADSettingsStep>();

        services.AddSiteDisplayDriver<AzureADSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenuAAD>();

        services.AddTransient<IConfigureOptions<AzureADSettings>, AzureADSettingsConfiguration>();

        services.AddTransient<IConfigureOptions<AuthenticationOptions>, AzureADOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<MicrosoftIdentityOptions>, AzureADOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<PolicySchemeOptions>, AzureADOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsConfiguration>();

        services.AddSingleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
[Feature(MicrosoftAuthenticationConstants.Features.AAD)]
public sealed class AzureADDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AzureADDeploymentSource, AzureADDeploymentStep, AzureADDeploymentStepDriver>();
    }
}
