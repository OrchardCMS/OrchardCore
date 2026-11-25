using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Users;

internal static class CookieNameServiceCollectionExtensions
{
    /// <summary>
    /// Sets the name of the cookie of an authentication scheme from the configured <see cref="CookieNameOptions"/>.
    /// </summary>
    public static IServiceCollection ConfigureCookieName(this IServiceCollection services, string scheme, Func<CookieNameOptions, string> cookieName)
    {
        services.AddOptions<CookieAuthenticationOptions>(scheme)
            .Configure<IOptions<CookieNameOptions>>((options, cookieNames) => options.Cookie.Name = cookieName(cookieNames.Value));

        return services;
    }
}
