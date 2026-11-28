using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Users;

/// <summary>
/// Binds the <see cref="UsersCookieOptions"/> from the tenant configuration, and gives the default value to every
/// name that is not configured.
/// </summary>
internal sealed class UsersCookieOptionsSetup : IConfigureOptions<UsersCookieOptions>, IPostConfigureOptions<UsersCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public UsersCookieOptionsSetup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public void Configure(UsersCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:Users").Bind(options);

    public void PostConfigure(string name, UsersCookieOptions options)
    {
        var encodedTenantName = HttpUtility.UrlEncode(_shellSettings.Name);

        options.Authentication = GetValueOrDefault(options.Authentication, UsersCookieOptions.DefaultAuthenticationPrefix + encodedTenantName);
        options.EmailConfirmation = GetValueOrDefault(options.EmailConfirmation, UsersCookieOptions.DefaultEmailConfirmationPrefix + encodedTenantName);
        options.Passkey = GetValueOrDefault(options.Passkey, UsersCookieOptions.DefaultPasskeyPrefix + encodedTenantName);
        options.ExternalAuthentication = GetValueOrDefault(options.ExternalAuthentication, UsersCookieOptions.DefaultExternalAuthentication);
        options.TwoFactorUserId = GetValueOrDefault(options.TwoFactorUserId, UsersCookieOptions.DefaultTwoFactorUserId);
        options.TwoFactorRememberMe = GetValueOrDefault(options.TwoFactorRememberMe, UsersCookieOptions.DefaultTwoFactorRememberMe);
    }

    private static string GetValueOrDefault(string value, string defaultValue)
        => string.IsNullOrWhiteSpace(value) ? defaultValue : value;
}
