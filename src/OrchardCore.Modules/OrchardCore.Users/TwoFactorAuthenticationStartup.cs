using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
using System;

namespace OrchardCore.Users;

[Feature(UserConstants.Features.TwoFactorAuthentication)]
public class TwoFactorAuthenticationStartup : StartupBase
{
    private static readonly string _twoFactorControllerName = typeof(TwoFactorAuthenticationController).ControllerName();

    private UserOptions _userOptions;

    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<TwoFactorAuthenticationAuthorizationFilter>();
        });

        services.AddScoped<IUserClaimsProvider, TwoFactorAuthenticationClaimsProvider>();
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
}

[Feature(UserConstants.Features.EmailAuthenticator)]
public class EmailAuthenticatorStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TwoFactorOptions>(options =>
        {
            options.Providers.Add(TokenOptions.DefaultEmailProvider);
        });

        services.AddScoped<IDisplayDriver<TwoFactorMethod>, TwoFactorMethodLoginEmailDisplayDriver>();
        services.AddScoped<IDisplayDriver<ISite>, EmailAuthenticatorLoginSettingsDisplayDriver>();
    }
}

[Feature(UserConstants.Features.SmsAuthenticator)]
public class SmsAuthenticatorStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<TwoFactorOptions>(options =>
        {
            options.Providers.Add(TokenOptions.DefaultPhoneProvider);
        });

        services.AddScoped<IDisplayDriver<TwoFactorMethod>, TwoFactorMethodLoginSmsDisplayDriver>();
        services.AddScoped<IDisplayDriver<ISite>, SmsAuthenticatorLoginSettingsDisplayDriver>();
    }
}
