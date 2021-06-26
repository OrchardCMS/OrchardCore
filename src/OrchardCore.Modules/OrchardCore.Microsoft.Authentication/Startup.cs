using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Microsoft.Authentication.Configuration;
using OrchardCore.Microsoft.Authentication.Deployment;
using OrchardCore.Microsoft.Authentication.Drivers;
using OrchardCore.Microsoft.Authentication.Recipes;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

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

                #pragma warning disable CS0618
                // The net5.0 5.0.3 build obsoletes 'AzureADOptions' and 'AzureADDefaults', 'Microsoft.Identity.Web' should be used instead.
                // The build warning is disabled temporarily until the code can be migrated.

                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, AzureADOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<AzureADOptions>, AzureADOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<PolicySchemeOptions>, AzureADOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<OpenIdConnectOptions>, OpenIdConnectOptionsConfiguration>(),

                // Restore the obsolete warning disabled above
                #pragma warning restore CS0618

                // Built-in initializers:
                ServiceDescriptor.Singleton<IPostConfigureOptions<OpenIdConnectOptions>, OpenIdConnectPostConfigureOptions>(),
            });
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IDisplayDriver<DeploymentStep>, AzureADDeploymentStepDriver>();
            services.AddTransient<IDeploymentSource, AzureADDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory, DeploymentStepFactory<AzureADDeploymentStep>>();
        }
    }
}
