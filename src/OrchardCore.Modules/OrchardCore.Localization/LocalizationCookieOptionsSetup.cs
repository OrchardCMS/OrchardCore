using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Localization;

/// <summary>
/// Binds the <see cref="LocalizationCookieOptions"/> from the tenant configuration, and gives the default value to
/// every name that is not configured.
/// </summary>
internal sealed class LocalizationCookieOptionsSetup : IConfigureOptions<LocalizationCookieOptions>, IPostConfigureOptions<LocalizationCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public LocalizationCookieOptionsSetup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public void Configure(LocalizationCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:Localization").Bind(options);

    public void PostConfigure(string name, LocalizationCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Culture))
        {
            options.Culture = LocalizationCookieOptions.DefaultCulture;
        }

        if (string.IsNullOrWhiteSpace(options.AdminCulture))
        {
            options.AdminCulture = LocalizationCookieOptions.DefaultAdminCulturePrefix + _shellSettings.VersionId;
        }
    }
}