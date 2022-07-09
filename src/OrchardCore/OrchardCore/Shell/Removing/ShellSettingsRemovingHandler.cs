using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellSettingsRemovingHandler : ShellRemovingHostHandler
{
    private readonly IShellHost _shellHost;
    private readonly ILogger _logger;

    public ShellSettingsRemovingHandler(IShellHost shellHost, ILogger<ShellSettingsRemovingHandler> logger)
    {
        _shellHost = shellHost;
        _logger = logger;
    }

    /// <summary>
    /// Removes the shell settings of the provided tenant.
    /// </summary>
    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        try
        {
            await _shellHost.RemoveShellSettingsAsync(context.ShellSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the shell settings of tenant '{TenantName}'.",
                context.ShellSettings.Name);

            context.ErrorMessage = $"Failed to remove the shell settings.";
            context.Error = ex;
        }
    }
}
