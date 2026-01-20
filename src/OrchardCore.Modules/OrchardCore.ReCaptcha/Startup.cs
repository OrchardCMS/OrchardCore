using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.ReCaptcha.Configuration;
using OrchardCore.ReCaptcha.Core;
using OrchardCore.ReCaptcha.Drivers;
using OrchardCore.ReCaptcha.Users.Handlers;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.Deployment;
using OrchardCore.Users;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.ReCaptcha;

[Feature("OrchardCore.ReCaptcha")]
public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddReCaptcha();
        services.AddSiteDisplayDriver<ReCaptchaSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
        services.AddPermissionProvider<Permissions>();
    }
}

[Feature("OrchardCore.ReCaptcha")]
[RequireFeatures("OrchardCore.Deployment")]
public sealed class DeploymentStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteSettingsPropertyDeploymentStep<ReCaptchaSettings, DeploymentStartup>(S => S["ReCaptcha settings"], S => S["Exports the ReCaptcha settings."]);
    }
}

[Feature("OrchardCore.ReCaptcha.Users")]
public sealed class UsersStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IRegistrationFormEvents, RegistrationFormEventHandler>();
        services.AddScoped<ILoginFormEvent, LoginFormEventEventHandler>();
        services.AddScoped<IPasswordRecoveryFormEvents, PasswordRecoveryFormEventEventHandler>();
        services.AddDisplayDriver<LoginForm, ReCaptchaLoginFormDisplayDriver>();
    }
}

[Feature("OrchardCore.ReCaptcha.Users")]
[RequireFeatures(UserConstants.Features.ResetPassword)]
public sealed class UsersResetPasswordStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<ForgotPasswordForm, ReCaptchaForgotPasswordFormDisplayDriver>();
        services.AddDisplayDriver<ResetPasswordForm, ReCaptchaResetPasswordFormDisplayDriver>();
    }
}

[Feature("OrchardCore.ReCaptcha.Users")]
[RequireFeatures(UserConstants.Features.UserRegistration)]
public sealed class UsersRegistrationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<RegisterUserForm, RegisterUserFormDisplayDriver>();
    }
}
