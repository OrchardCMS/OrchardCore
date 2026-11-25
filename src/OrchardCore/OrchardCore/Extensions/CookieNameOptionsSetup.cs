using System.Web;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Extensions;

/// <summary>
/// Gives the default value to every cookie name of the tenant that is not configured.
/// </summary>
internal sealed class CookieNameOptionsSetup : IPostConfigureOptions<CookieNameOptions>
{
    private readonly ShellSettings _shellSettings;

    public CookieNameOptionsSetup(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public void PostConfigure(string name, CookieNameOptions options)
    {
        var encodedTenantName = HttpUtility.UrlEncode(_shellSettings.Name);

        options.Antiforgery = GetValueOrDefault(options.Antiforgery, CookieNames.AntiforgeryPrefix + _shellSettings.VersionId);
        options.Authentication = GetValueOrDefault(options.Authentication, CookieNames.AuthenticationPrefix + encodedTenantName);
        options.EmailConfirmation = GetValueOrDefault(options.EmailConfirmation, CookieNames.EmailConfirmationPrefix + encodedTenantName);
        options.Passkey = GetValueOrDefault(options.Passkey, CookieNames.PasskeyPrefix + encodedTenantName);
        options.ExternalAuthentication = GetValueOrDefault(options.ExternalAuthentication, CookieNames.ExternalAuthentication);
        options.TwoFactorUserId = GetValueOrDefault(options.TwoFactorUserId, CookieNames.TwoFactorUserId);
        options.TwoFactorRememberMe = GetValueOrDefault(options.TwoFactorRememberMe, CookieNames.TwoFactorRememberMe);
        options.Notify = GetValueOrDefault(options.Notify, CookieNames.Notify);
        options.TempData = GetValueOrDefault(options.TempData, CookieNames.TempData);
        options.Culture = GetValueOrDefault(options.Culture, CookieNames.Culture);
        options.Consent = GetValueOrDefault(options.Consent, CookieNames.Consent);
        options.AdminCulture = GetValueOrDefault(options.AdminCulture, CookieNames.AdminCulturePrefix + _shellSettings.VersionId);
        options.AdminListLayout = GetValueOrDefault(options.AdminListLayout, CookieNames.AdminListLayout);
        options.AdminTheme = GetValueOrDefault(options.AdminTheme, _shellSettings.Name + CookieNames.AdminThemeSuffix);
        options.AdminScheme = GetValueOrDefault(options.AdminScheme, _shellSettings.Name + CookieNames.AdminSchemeSuffix);
        options.AdminPreferences = GetValueOrDefault(options.AdminPreferences, _shellSettings.Name + CookieNames.AdminPreferencesSuffix);
    }

    private static string GetValueOrDefault(string value, string defaultValue)
        => string.IsNullOrWhiteSpace(value) ? defaultValue : value;
}
