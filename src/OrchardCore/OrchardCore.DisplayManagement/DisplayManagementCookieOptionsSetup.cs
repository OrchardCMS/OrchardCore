using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.DisplayManagement;

/// <summary>
/// Binds the <see cref="DisplayManagementCookieOptions"/> from the tenant configuration, and gives the default value
/// to every name that is not configured.
/// </summary>
internal sealed class DisplayManagementCookieOptionsSetup :
    IConfigureOptions<DisplayManagementCookieOptions>,
    IPostConfigureOptions<DisplayManagementCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public DisplayManagementCookieOptionsSetup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(DisplayManagementCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:DisplayManagement").Bind(options);

    public void PostConfigure(string name, DisplayManagementCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Notify))
        {
            options.Notify = DisplayManagementCookieOptions.DefaultNotify;
        }
    }
}
