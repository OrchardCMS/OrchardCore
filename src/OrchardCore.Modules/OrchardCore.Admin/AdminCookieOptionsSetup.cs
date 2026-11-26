using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Admin;

/// <summary>
/// Binds the <see cref="AdminCookieOptions"/> from the tenant configuration, and gives the default value to every
/// name that is not configured.
/// </summary>
internal sealed class AdminCookieOptionsSetup : IConfigureOptions<AdminCookieOptions>, IPostConfigureOptions<AdminCookieOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public AdminCookieOptionsSetup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(AdminCookieOptions options)
        => _shellConfiguration.GetSection("Cookies:Admin").Bind(options);

    public void PostConfigure(string name, AdminCookieOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ListLayout))
        {
            options.ListLayout = AdminCookieOptions.DefaultListLayout;
        }
    }
}