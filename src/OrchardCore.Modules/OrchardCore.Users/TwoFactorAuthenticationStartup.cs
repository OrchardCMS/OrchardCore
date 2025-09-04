using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Security.Permissions;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Endpoints.EmailAuthenticator;
using OrchardCore.Users.Endpoints.SmsAuthenticator;
using OrchardCore.Users.Events;
using OrchardCore.Users.Filters;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users;

[Feature(UserConstants.Features.TwoFactorAuthentication)]
public sealed class TwoFactorAuthenticationStartup : StartupBase
{
    private static readonly string _twoFactorControllerName = typeof(TwoFactorAuthenticationController).ControllerName();

    private UserOptions _userOptions;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<TwoFactorAuthenticationAuthorizationFilter>();
        });

        services.AddDisplayDriver<User, UserTwoFactorDisplayDriver>();
        services.AddScoped<IUserClaimsProvider, TwoFactorAuthenticationClaimsProvider>();
        services.AddDisplayDriver<UserMenu, TwoFactorUserMenuDisplayDriver>();
        services.AddSiteDisplayDriver<TwoFactorLoginSettingsDisplayDriver>();
        services.AddDisplayDriver<TwoFactorMethod, TwoFactorMethodDisplayDriver>();
        services.AddPermissionProvider<TwoFactorPermissionProvider>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        _userOptions ??= serviceProvider.GetRequiredService<IOptions<UserOptions>>().Value;

        routes.MapAreaControllerRoute(
                name: "LoginWithTwoFactorAuthentication",
                areaName: UserConstants.Features.Users,
                pattern: "LoginWithTwoFactorAuthentication",
                defaults: new
                {
                    controller = _twoFactorControllerName,
                    action = nameof(TwoFactorAuthenticationController.LoginWithTwoFactorAuthentication),
                }
            );

        routes.MapAreaControllerRoute(
            name: "TwoFactorAuthentication",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.TwoFactorAuthenticationPath,
            defaults: new
            {
                controller = _twoFactorControllerName,
                action = nameof(TwoFactorAuthenticationController.Index),
            }
        );

        routes.MapAreaControllerRoute(
            name: "LoginWithRecoveryCode",
            areaName: UserConstants.Features.Users,
            pattern: "LoginWithRecoveryCode",
            defaults: new
            {
                controller = _twoFactorControllerName,
                action = nameof(TwoFactorAuthenticationController.LoginWithRecoveryCode),
            }
        );

        routes.MapAreaControllerRoute(
            name: "GenerateRecoveryCodes",
            areaName: UserConstants.Features.Users,
            pattern: "GenerateRecoveryCodes",
            defaults: new
            {
                controller = _twoFactorControllerName,
                action = nameof(TwoFactorAuthenticationController.GenerateRecoveryCodes),
            }
        );

        routes.MapAreaControllerRoute(
            name: "ShowRecoveryCodes",
            areaName: UserConstants.Features.Users,
            pattern: "ShowRecoveryCodes",
            defaults: new
            {
                controller = _twoFactorControllerName,
                action = nameof(TwoFactorAuthenticationController.ShowRecoveryCodes),
            }
        );

        routes.MapAreaControllerRoute(
            name: "DisableTwoFactorAuthentication",
            areaName: UserConstants.Features.Users,
            pattern: "DisableTwoFactorAuthentication",
            defaults: new
            {
                controller = _twoFactorControllerName,
                action = nameof(TwoFactorAuthenticationController.DisableTwoFactorAuthentication),
            }
        );
    }
}

[RequireFeatures("OrchardCore.Roles", UserConstants.Features.TwoFactorAuthentication)]
public sealed class RoleTwoFactorAuthenticationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<RoleLoginSettingsDisplayDriver>();
        services.AddScoped<ITwoFactorAuthenticationHandler, RoleTwoFactorAuthenticationHandler>();
    }
}

[Feature(UserConstants.Features.AuthenticatorApp)]
[RequireFeatures(UserConstants.Features.TwoFactorAuthentication)]
public sealed class AuthenticatorAppStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var authenticatorProviderType = typeof(AuthenticatorTokenProvider<>).MakeGenericType(typeof(IUser));
        services.AddTransient(authenticatorProviderType);

        services.Configure<IdentityOptions>(options =>
        {
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            options.Tokens.ProviderMap[TokenOptions.DefaultAuthenticatorProvider] = new TokenProviderDescriptor(authenticatorProviderType);
        });

        services.AddTransient<IConfigureOptions<TwoFactorOptions>, AuthenticatorAppProviderTwoFactorOptionsConfiguration>();
        services.AddSiteDisplayDriver<AuthenticatorAppLoginSettingsDisplayDriver>();
        services.AddDisplayDriver<TwoFactorMethod, TwoFactorMethodLoginAuthenticationAppDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var controllerName = typeof(AuthenticatorAppController).ControllerName();

        routes.MapAreaControllerRoute(
            name: "ConfigureAuthenticatorApp",
            areaName: UserConstants.Features.Users,
            pattern: "Authenticator/Configure/App",
            defaults: new
            {
                controller = controllerName,
                action = nameof(AuthenticatorAppController.Index),
            }
        );

        routes.MapAreaControllerRoute(
            name: "RemoveAuthenticatorApp",
            areaName: UserConstants.Features.Users,
            pattern: "Authenticator/Reset/App",
            defaults: new
            {
                controller = controllerName,
                action = nameof(AuthenticatorAppController.Reset),
            }
        );
    }
}

[Feature(UserConstants.Features.EmailAuthenticator)]
public sealed class EmailAuthenticatorStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var emailProviderType = typeof(EmailTokenProvider<>).MakeGenericType(typeof(IUser));
        services.AddTransient(emailProviderType)
            .Configure<TwoFactorOptions>(options => options.Providers.Add(TokenOptions.DefaultEmailProvider))
            .Configure<IdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[TokenOptions.DefaultEmailProvider] = new TokenProviderDescriptor(emailProviderType);
            });

        services.AddDisplayDriver<TwoFactorMethod, TwoFactorMethodLoginEmailDisplayDriver>();
        services.AddSiteDisplayDriver<EmailAuthenticatorLoginSettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddEmailSendCodeEndpoint<EmailAuthenticatorStartup>();
    }
}

[Feature(UserConstants.Features.SmsAuthenticator)]
public sealed class SmsAuthenticatorStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        var phoneNumberProviderType = typeof(PhoneNumberTokenProvider<>).MakeGenericType(typeof(IUser));
        services.AddTransient(phoneNumberProviderType);
        services.Configure<IdentityOptions>(options =>
        {
            options.Tokens.ChangePhoneNumberTokenProvider = TokenOptions.DefaultPhoneProvider;
            options.Tokens.ProviderMap[TokenOptions.DefaultPhoneProvider] = new TokenProviderDescriptor(phoneNumberProviderType);
        });

        services.AddTransient<IConfigureOptions<TwoFactorOptions>, PhoneProviderTwoFactorOptionsConfiguration>();
        services.AddDisplayDriver<TwoFactorMethod, TwoFactorMethodLoginSmsDisplayDriver>();
        services.AddSiteDisplayDriver<SmsAuthenticatorLoginSettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.AddSmsSendCodeEndpoint<SmsAuthenticatorStartup>();
    }
}
