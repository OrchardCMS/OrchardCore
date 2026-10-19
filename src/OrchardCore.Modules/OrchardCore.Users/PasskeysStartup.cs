using System.Web;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RateLimits;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Drivers;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users;

[Feature(UserConstants.Features.Passkeys)]
public sealed class PasskeysStartup : StartupBase
{
    private static readonly string s_passkeyControllerName = typeof(PasskeyController).ControllerName();

    private readonly string _tenantName;

    public PasskeysStartup(ShellSettings shellSettings)
    {
        _tenantName = shellSettings.Name;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<IdentityPasskeyOptions>, PasskeyOptionsConfiguration>();
        services.AddScoped<PasskeyService>();

        // A short-lived cookie that holds the state of a passkey registration or sign-in. Like the 'Identity.TwoFactorUserId' cookie,
        // it doesn't sign the user in.
        services.AddAuthentication()
            .AddCookie(UserConstants.PasskeyAuthenticationScheme, options =>
            {
                options.Cookie.Name = "orchpasskey_" + HttpUtility.UrlEncode(_tenantName);
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.SlidingExpiration = false;
            });

        // The state must outlive the time the browser waits for the user to use their passkey.
        services.AddOptions<CookieAuthenticationOptions>(UserConstants.PasskeyAuthenticationScheme)
            .Configure<IOptions<IdentityPasskeyOptions>>((options, passkeyOptions) =>
            {
                options.ExpireTimeSpan = passkeyOptions.Value.AuthenticatorTimeout + TimeSpan.FromMinutes(1);
            });

        services.AddDisplayDriver<LoginForm, PasskeyLoginFormDisplayDriver>();
        services.AddDisplayDriver<UserMenu, PasskeyUserMenuDisplayDriver>();
        services.AddDisplayDriver<User, UserPasskeyDisplayDriver>();
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Passkeys",
            areaName: UserConstants.Features.Users,
            pattern: "Passkeys",
            defaults: new
            {
                controller = s_passkeyControllerName,
                action = nameof(PasskeyController.Index),
            }
        );

        routes.MapAreaControllerRoute(
            name: RateLimitRouteNames.PasskeyLogin,
            areaName: UserConstants.Features.Users,
            pattern: "Passkeys/Login",
            defaults: new
            {
                controller = s_passkeyControllerName,
                action = nameof(PasskeyController.Login),
            }
        );
    }
}

[Feature(UserConstants.Features.Passkeys)]
[RequireFeatures(UserConstants.Features.TwoFactorAuthentication)]
public sealed class PasskeysTwoFactorAuthenticationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<PasskeyTwoFactorTokenProvider>()
            .Configure<TwoFactorOptions>(options => options.Providers.Add(UserConstants.PasskeyTwoFactorProvider))
            .Configure<IdentityOptions>(options =>
            {
                options.Tokens.ProviderMap[UserConstants.PasskeyTwoFactorProvider] = new TokenProviderDescriptor(typeof(PasskeyTwoFactorTokenProvider));
            });

        services.AddDisplayDriver<TwoFactorMethod, TwoFactorMethodLoginPasskeyDisplayDriver>();
    }
}

[Feature(UserConstants.Features.Passkeys)]
[RequireFeatures("OrchardCore.RateLimits")]
public sealed class PasskeysRateLimitsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<RateLimitsOptions>(options =>
        {
            options.AddRouteRateLimit(RateLimitRouteNames.PasskeyLogin, HttpMethods.Post, RateLimitPartitionHelpers.CreateSlidingWindowPerIpPolicy(UserRateLimiterPolicyNames.PasswordAuthentication, 10));
        });
    }
}
