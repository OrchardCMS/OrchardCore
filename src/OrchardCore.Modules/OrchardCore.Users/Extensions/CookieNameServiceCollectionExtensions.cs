using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users;

internal static class CookieNameServiceCollectionExtensions
{
    /// <summary>
    /// Sets the name of the cookie of an authentication scheme from the configured <see cref="UsersCookieOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureCookieName(this IServiceCollection services, string scheme, Func<UsersCookieOptions, string> cookieName)
    {
        services.AddOptions<CookieAuthenticationOptions>(scheme)
            .Configure<IOptions<UsersCookieOptions>>((options, cookieOptions) => options.Cookie.Name = cookieName(cookieOptions.Value));

        return services;
    }
}
