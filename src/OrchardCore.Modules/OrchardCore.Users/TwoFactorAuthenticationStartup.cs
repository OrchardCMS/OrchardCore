using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Filters;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users;

[Feature(UserConstants.Features.TwoFactorAuthentication)]
public class TwoFactorAuthenticationStartup : StartupBase
{
    private static readonly string _twoFactorControllerName = typeof(TwoFactorAuthenticationController).ControllerName();

    private readonly AdminOptions _adminOptions;

    private UserOptions _userOptions;

    public TwoFactorAuthenticationStartup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<TwoFactorAuthenticationAuthorizationFilter>();
        });

        services.AddScoped<IDisplayDriver<UserMenu>, TwoFactorUserMenuDisplayDriver>();
        services.AddScoped<IDisplayDriver<ISite>, TwoFactorLoginSettingsDisplayDriver>();
        services.AddScoped<IDisplayDriver<TwoFactorMethod>, TwoFactorMethodDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        _userOptions ??= serviceProvider.GetRequiredService<IOptions<UserOptions>>().Value;

        routes.MapAreaControllerRoute(
            name: "LoginWithTwoFactorAuthentication",
            areaName: UserConstants.Features.Users,
            pattern: "LoginWithTwoFactorAuthentication",
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.LoginWithTwoFactorAuthentication) }
        );

        routes.MapAreaControllerRoute(
            name: "TwoFactorAuthentication",
            areaName: UserConstants.Features.Users,
            pattern: _userOptions.TwoFactorAuthenticationPath,
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "GenerateRecoveryCodes",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/GenerateRecoveryCodes",
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.GenerateRecoveryCodes) }
        );

        routes.MapAreaControllerRoute(
            name: "ShowRecoveryCodes",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/ShowRecoveryCodes",
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.ShowRecoveryCodes) }
        );

        routes.MapAreaControllerRoute(
            name: "DisableTwoFactorAuthentication",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/DisableTwoFactorAuthentication",
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.DisableTwoFactorAuthentication) }
        );

        routes.MapAreaControllerRoute(
            name: "EnableTwoFactorAuthentication",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/EnableTwoFactorAuthentication",
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.EnableTwoFactorAuthentication) }
        );
        routes.MapAreaControllerRoute(
            name: "LoginWithRecoveryCode",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/LoginWithRecoveryCode",
            defaults: new { controller = _twoFactorControllerName, action = nameof(TwoFactorAuthenticationController.LoginWithRecoveryCode) }
        );
    }
}

[RequireFeatures("OrchardCore.Roles", UserConstants.Features.TwoFactorAuthentication)]
public class RoleTwoFactorAuthenticationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDisplayDriver<ISite>, RoleLoginSettingsDisplayDriver>();
        services.AddScoped<ITwoFactorAuthenticationHandler, RoleTwoFactorAuthenticationHandler>();
    }
}

[Feature(UserConstants.Features.AuthenticatorApp)]
[RequireFeatures(UserConstants.Features.TwoFactorAuthentication)]
public class AuthenticatorAppStartup : StartupBase
{
    private static readonly string _authenticatorAppControllerName = typeof(AuthenticatorAppController).ControllerName();

    private readonly AdminOptions _adminOptions;

    public AuthenticatorAppStartup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        var authenticatorProviderType = typeof(AuthenticatorTokenProvider<>).MakeGenericType(typeof(IUser));
        services.AddTransient(authenticatorProviderType);
        services.Configure<IdentityOptions>(options =>
        {
            options.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            options.Tokens.ProviderMap.TryAdd(TokenOptions.DefaultAuthenticatorProvider, new TokenProviderDescriptor(authenticatorProviderType));
        });

        services.Configure<TwoFactorOptions>(options =>
        {
            options.Providers.Add(TokenOptions.DefaultAuthenticatorProvider);
        });
        services.AddScoped<IDisplayDriver<ISite>, AuthenticatorAppLoginSettingsDisplayDriver>();
        services.AddScoped<IDisplayDriver<TwoFactorMethod>, TwoFactorMethodLoginAuthenticationAppDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "ConfigureAuthenticatorApp",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Configure/App",
            defaults: new { controller = _authenticatorAppControllerName, action = nameof(AuthenticatorAppController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "RemoveAuthenticatorApp",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Reset/App",
            defaults: new { controller = _authenticatorAppControllerName, action = nameof(AuthenticatorAppController.Reset) }
        );
    }
}

[Feature(UserConstants.Features.EmailAuthenticator)]
public class EmailAuthenticatorStartup : StartupBase
{
    private static readonly string _emailAuthenticatorControllerName = typeof(EmailAuthenticatorController).ControllerName();
    private readonly AdminOptions _adminOptions;

    public EmailAuthenticatorStartup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TwoFactorOptions>(options =>
        {
            options.Providers.Add(TokenOptions.DefaultEmailProvider);
        });

        services.AddScoped<IDisplayDriver<TwoFactorMethod>, TwoFactorMethodLoginEmailDisplayDriver>();
        services.AddScoped<IDisplayDriver<ISite>, EmailAuthenticatorLoginSettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "ConfigureEmailAuthenticator",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Configure/Email",
            defaults: new { controller = _emailAuthenticatorControllerName, action = nameof(EmailAuthenticatorController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "ConfigureEmailAuthenticatorRequestCode",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Configure/Email/RequestCode",
            defaults: new { controller = _emailAuthenticatorControllerName, action = nameof(EmailAuthenticatorController.RequestCode) }
        );

        routes.MapAreaControllerRoute(
            name: "ConfigureEmailAuthenticatorValidateCode",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Configure/Email/ValidateCode",
            defaults: new { controller = _emailAuthenticatorControllerName, action = nameof(EmailAuthenticatorController.ValidateCode) }
        );
    }
}

[Feature(UserConstants.Features.SmsAuthenticator)]
public class SmsAuthenticatorStartup : StartupBase
{
    private static readonly string _smsAuthenticatorControllerName = typeof(SmsAuthenticatorController).ControllerName();
    private readonly AdminOptions _adminOptions;

    public SmsAuthenticatorStartup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TwoFactorOptions>(options =>
        {
            options.Providers.Add(TokenOptions.DefaultPhoneProvider);
        });

        services.AddScoped<IDisplayDriver<TwoFactorMethod>, TwoFactorMethodLoginSmsDisplayDriver>();
        services.AddScoped<IDisplayDriver<ISite>, SmsAuthenticatorLoginSettingsDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "ConfigureSmsAuthenticator",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Configure/Sms",
            defaults: new { controller = _smsAuthenticatorControllerName, action = nameof(SmsAuthenticatorController.Index) }
        );

        routes.MapAreaControllerRoute(
            name: "ConfigureSmsAuthenticatorValidateCode",
            areaName: UserConstants.Features.Users,
            pattern: _adminOptions.AdminUrlPrefix + "/Authenticator/Configure/Sms/ValidateCode",
            defaults: new { controller = _smsAuthenticatorControllerName, action = nameof(SmsAuthenticatorController.ValidateCode) }
        );
    }
}
