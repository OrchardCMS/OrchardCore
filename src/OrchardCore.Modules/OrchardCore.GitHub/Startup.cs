using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.GitHub.Configuration;
using OrchardCore.GitHub.Drivers;
using OrchardCore.GitHub.Recipes;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Settings.Deployment;

namespace OrchardCore.GitHub
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
        }
    }

    [Feature(GitHubConstants.Features.GitHubAuthentication)]
    public class GitHubLoginStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGitHubAuthenticationService, GitHubAuthenticationService>();
            services.AddScoped<IDisplayDriver<ISite>, GitHubAuthenticationSettingsDisplayDriver>();
            services.AddScoped<INavigationProvider, AdminMenuGitHubLogin>();
            services.AddRecipeExecutionStep<GitHubAuthenticationSettingsStep>();
            // Register the options initializers required by the GitHub Handler.
            services.TryAddEnumerable(new[]
            {
                // Orchard-specific initializers:
                ServiceDescriptor.Transient<IConfigureOptions<AuthenticationOptions>, GitHubOptionsConfiguration>(),
                ServiceDescriptor.Transient<IConfigureOptions<GitHubOptions>, GitHubOptionsConfiguration>(),
                // Built-in initializers:
                ServiceDescriptor.Transient<IPostConfigureOptions<GitHubOptions>, OAuthPostConfigureOptions<GitHubOptions,GitHubHandler>>()
            });
        }

        [RequireFeatures("OrchardCore.Deployment")]
        public class DeploymentStartup : StartupBase
        {
            public override void ConfigureServices(IServiceCollection services)
            {
                services.AddTransient<IDeploymentSource, SiteSettingsPropertyDeploymentSource<GitHubAuthenticationSettings>>();
                services.AddScoped<IDisplayDriver<DeploymentStep>>(sp =>
                {
                    var S = sp.GetService<IStringLocalizer<DeploymentStartup>>();
                    return new SiteSettingsPropertyDeploymentStepDriver<GitHubAuthenticationSettings>(S["GitHub Authentication settings"], S["Exports the GitHub Authentication settings."]);
                });
                services.AddSingleton<IDeploymentStepFactory>(new SiteSettingsPropertyDeploymentStepFactory<GitHubAuthenticationSettings>());
            }
        }
    }
}
