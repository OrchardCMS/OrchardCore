using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

[Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
public sealed class MicrosoftAccountStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IPermissionProvider), typeof(Permissions), ServiceLifetime.Scoped));

        services.AddSingleton<IMicrosoftAccountService, MicrosoftAccountService>();
        services.AddSiteDisplayDriver<MicrosoftAccountSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenuMicrosoftAccount>();

        services.AddRecipeExecutionStep<MicrosoftAccountSettingsStep>();

        services.AddTransient<IConfigureOptions<MicrosoftAccountSettings>, MicrosoftAccountSettingsConfiguration>();

        // Register the options initializers required by the Microsoft Account Handler.
        services.TryAddEnumerable(new[]
        {
            // Orchard-specific initializers:
            ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, MicrosoftAccountOptionsConfiguration>(),
            ServiceDescriptor.Transient<IConfigureOptions<MicrosoftAccountOptions>, MicrosoftAccountOptionsConfiguration>(),
            // Built-in initializers:
            ServiceDescriptor.Transient<IPostConfigureOptions<MicrosoftAccountOptions>, OAuthPostConfigureOptions<MicrosoftAccountOptions, MicrosoftAccountHandler>>()
        });
    }
}

[Feature(MicrosoftAuthenticationConstants.Features.AAD)]
public sealed class AzureADStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.TryAddEnumerable(new ServiceDescriptor(typeof(IPermissionProvider), typeof(Permissions), ServiceLifetime.Scoped));

        services.AddSingleton<IAzureADService, AzureADService>();
        services.AddRecipeExecutionStep<AzureADSettingsStep>();

        services.AddSiteDisplayDriver<AzureADSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenuAAD>();

        services.AddTransient<IConfigureOptions<AzureADSettings>, AzureADSettingsConfiguration>();

        // Register the options initializers required by the Policy Scheme, Cookie and OpenId Connect Handler.
        services.TryAddEnumerable(new[]
        {
            // Orchard-specific initializers.
            ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, AzureADOptionsConfiguration>(),
            ServiceDescriptor.Transient<IConfigureOptions<MicrosoftIdentityOptions>, AzureADOptionsConfiguration>(),
            ServiceDescriptor.Transient<IConfigureOptions<PolicySchemeOptions>, AzureADOptionsConfiguration>(),
            ServiceDescriptor.Transient<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsConfiguration>(),

            // Built-in initializers:
            ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>(),
        });
    }
}

[RequireFeatures("OrchardCore.Deployment")]
[Feature(MicrosoftAuthenticationConstants.Features.AAD)]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<AzureADDeploymentSource, AzureADDeploymentStep, AzureADDeploymentStepDriver>();
    }
}
