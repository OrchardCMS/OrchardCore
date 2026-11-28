using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Themes.TheAdmin;

/// <summary>
/// Binds the <see cref="TheAdminCookieOptions"/> from the tenant configuration, and gives the default value to every
/// name that is not configured.
/// </summary>
internal sealed class TheAdminCookieOptionsSetup : IConfigureOptions<TheAdminCookieOptions>, IPostConfigureOptions<TheAdminCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;

    public TheAdminCookieOptionsSetup(IShellConfiguration shellConfiguration, ShellSettings shellSettings)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
    }

    public void Configure(TheAdminCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:TheAdmin").Bind(options);

    public void PostConfigure(string name, TheAdminCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Scheme))
        {
            options.Scheme = _shellSettings.Name + TheAdminCookieOptions.DefaultSchemeSuffix;
        }
    }
}