using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Data;
using OrchardCore.Json;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using OrchardCore.Users;
using OrchardCore.Users.Core.Json;
using OrchardCore.Users.Events;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Indexes;
using OrchardCore.Users.Services;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class UsersServiceCollectionExtensions
{
    /// <summary>
    /// Registers the users services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public static IServiceCollection AddUsers(this IServiceCollection services)
    {
        services.TryAddScoped<UserStore>();
        services.TryAddScoped<IUserStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserRoleStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserPasswordStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserEmailStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserSecurityStampStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserLoginStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserClaimStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserTwoFactorStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserTwoFactorRecoveryCodeStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserAuthenticatorKeyStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserAuthenticationTokenStore<IUser>>(sp => sp.GetRequiredService<UserStore>());
        services.TryAddScoped<IUserPhoneNumberStore<IUser>>(sp => sp.GetRequiredService<UserStore>());

        services.AddScoped<NullRoleStore>();
        services.TryAddScoped<IRoleClaimStore<IRole>>(sp => sp.GetRequiredService<NullRoleStore>());
        services.TryAddScoped<IRoleStore<IRole>>(sp => sp.GetRequiredService<NullRoleStore>());

        services.TryAddScoped<RoleManager<IRole>>();
        services.TryAddScoped<IRoleService, RoleService>();

        services.AddIndexProvider<UserIndexProvider>();
        services.AddIndexProvider<UserByLoginInfoIndexProvider>();
        services.AddIndexProvider<UserByClaimIndexProvider>();

        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserClaimsPrincipalFactory<IUser>, DefaultUserClaimsPrincipalProviderFactory>();
        services.AddScoped<IUserEventHandler, UserDisabledEventHandler>();

        services.AddScoped<ITwoFactorAuthenticationHandler, DefaultTwoFactorAuthenticationHandler>();
        services.AddScoped<ITwoFactorAuthenticationHandlerCoordinator, DefaultTwoFactorAuthenticationHandlerCoordinator>();

        services.Configure<DocumentJsonSerializerOptions>(options =>
        {
            options.SerializerOptions.Converters.Add(LoginInfoJsonConverter.Instance);
        });

        return services;
    }
}
