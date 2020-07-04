using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Drivers;
using OrchardCore.Microsoft.Authentication.Recipes;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.Microsoft.Authentication
{
    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    public class MicrosoftAccountStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IPermissionProvider), typeof(Permissions), ServiceLifetime.Scoped));

            services.AddSingleton<IMicrosoftAccountService, MicrosoftAccountService>();
            services.AddScoped<IDisplayDriver<ISite>, MicrosoftAccountSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuMicrosoftAccount>();
            services.AddRecipeExecutionStep<MicrosoftAccountSettingsStep>();
            // Register the options initializers required by the Microsoft Account Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, MicrosoftAccountOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<MicrosoftAccountOptions>, MicrosoftAccountOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<MicrosoftAccountOptions>, OAuthPostConfigureOptions<MicrosoftAccountOptions,MicrosoftAccountHandler>>()
            });
        }
    }

    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class MicrosoftAccountDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<MicrosoftAccountSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<MicrosoftAccountDeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<MicrosoftAccountSettings>(S["Microsoft Authentication settings"], S["Exports the Microsoft Authentication settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<MicrosoftAccountSettings>());
        }
    }

    [Feature(MicrosoftAuthenticationConstants.Features.AAD)]
    public class AzureADStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.TryAddEnumerable(new ServiceDescriptor(typeof(IPermissionProvider), typeof(Permissions), ServiceLifetime.Scoped));

            services.AddSingleton<IAzureADService, AzureADService>();
            services.AddRecipeExecutionStep<AzureADSettingsStep>();
            services.AddScoped<IDisplayDriver<ISite>, AzureADSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuAAD>();
            // Register the options initializers required by the Policy Scheme, Cookie and OpenId Connect Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, AzureADOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<AzureADOptions>, AzureADOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<PolicySchemeOptions>, AzureADOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>(),
            });
        }
    }

    [Feature(MicrosoftAuthenticationConstants.Features.AAD)]
    [RequireFeatures("OrchardCore.Deployment")]
    public class AzureADDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<AzureADSettings>>();
            services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
            {
                var S = sp.GetService<IStringLocalizer<AzureADDeploymentStartup>>();
                return new SiteSettingsPropertyDeploymentStepDriver<AzureADSettings>(S["Azure AD settings"], S["Exports the Azure AD settings."]);
            });
            services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<AzureADSettings>());
        }
    }
}
