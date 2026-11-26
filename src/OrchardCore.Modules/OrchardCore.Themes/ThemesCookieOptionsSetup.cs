using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Themes;

/// <summary>
/// Binds the <see cref="ThemesCookieOptions"/> from the tenant configuration, and gives the default value to every
/// name that is not configured.
/// </summary>
internal sealed class ThemesCookieOptionsSetup : IConfigureOptions<ThemesCookieOptions>, IPostConfigureOptions<ThemesCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public ThemesCookieOptionsSetup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public void Configure(ThemesCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:Themes").Bind(options);

    public void PostConfigure(string name, ThemesCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AdminTheme))
        {
            options.AdminTheme = _shellSettings.Name + ThemesCookieOptions.DefaultAdminThemeSuffix;
        }

        if (string.IsNullOrWhiteSpace(options.AdminPreferences))
        {
            options.AdminPreferences = _shellSettings.Name + ThemesCookieOptions.DefaultAdminPreferencesSuffix;
        }
    }
}