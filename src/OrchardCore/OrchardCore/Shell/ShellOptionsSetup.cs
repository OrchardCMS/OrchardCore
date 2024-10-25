using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell;

/// <summary>
/// Sets up default options for <see cref="ShellOptions"/>.
/// </summary>
public sealed class ShellOptionsSetup : IConfigureOptions<ShellOptions>
{
    private readonly IHostEnvironment _hostingEnvironment;

    public ShellOptionsSetup(IHostEnvironment hostingEnvironment)
    {
        _hostingEnvironment = hostingEnvironment;
    }

    public void Configure(ShellOptions options)
    {
        var appData = System.Environment.GetEnvironmentVariable(ShellOptionConstants.OrchardAppData);

        if (!string.IsNullOrEmpty(appData))
        {
            options.ShellsApplicationDataPath = Path.Combine(_hostingEnvironment.ContentRootPath, appData);
        }
        else
        {
            options.ShellsApplicationDataPath = Path.Combine(_hostingEnvironment.ContentRootPath, ShellOptionConstants.DefaultAppDataPath);
        }

        options.ShellsContainerName = ShellOptionConstants.DefaultSitesPath;
    }
}
