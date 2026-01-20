using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        services.AddPermissionProvider<Permissions>();

        services.AddSingleton<IMicrosoftAccountService, MicrosoftAccountService>();
        services.AddSiteDisplayDriver<MicrosoftAccountSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenuMicrosoftAccount>();

        services.AddRecipeExecutionStep<MicrosoftAccountSettingsStep>();

        services.AddTransient<IConfigureOptions<MicrosoftAccountSettings>, MicrosoftAccountSettingsConfiguration>();
        services.AddTransient<IConfigureOptions<AuthenticationOptions>, MicrosoftAccountOptionsConfiguration>();
        services.AddTransient<IConfigureOptions<MicrosoftAccountOptions>, MicrosoftAccountOptionsConfiguration>();
        services.AddTransient<IPostConfigureOptions<MicrosoftAccountOptions>, OAuthPostConfigureOptions<MicrosoftAccountOptions, MicrosoftAccountHandler>>();
    }
}

[RequireFeatures("OrchardCore.Deployment")]
[Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
public sealed class MicrosoftAccountDeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDeployment<MicrosoftAccountDeploymentSource, MicrosoftAccountDeploymentStep, MicrosoftAccountDeploymentStepDriver>();
    }
}
