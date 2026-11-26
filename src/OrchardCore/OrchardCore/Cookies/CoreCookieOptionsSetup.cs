using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Cookies;

/// <summary>
/// Binds the <see cref="CoreCookieOptions"/> from the tenant configuration, and gives the default value to every
/// name that is not configured.
/// </summary>
internal sealed class CoreCookieOptionsSetup : IConfigureOptions<CoreCookieOptions>, IPostConfigureOptions<CoreCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public CoreCookieOptionsSetup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public void Configure(CoreCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:Core").Bind(options);

    public void PostConfigure(string name, CoreCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Antiforgery))
        {
            options.Antiforgery = CoreCookieOptions.DefaultAntiforgeryPrefix + _shellSettings.VersionId;
        }

        if (string.IsNullOrWhiteSpace(options.TempData))
        {
            options.TempData = CoreCookieOptions.DefaultTempData;
        }

        if (string.IsNullOrWhiteSpace(options.Consent))
        {
            options.Consent = CoreCookieOptions.DefaultConsent;
        }
    }
}
