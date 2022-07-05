using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellSettingsRemovingHandler : IShellRemovingHandler
{
    private readonly IShellHost _shellHost;
    private readonly ShellOptions _shellOptions;
    private readonly ILogger _logger;

    public ShellSettingsRemovingHandler(
        IShellHost shellHost,
        IOptions<ShellOptions> shellOptions,
        ILogger<ShellSettingsRemovingHandler> logger)
    {
        _shellHost = shellHost;
        _shellOptions = shellOptions.Value;
        _logger = logger;
    }

    public async Task<ShellRemovingResult> RemovingAsync(string tenant)
    {
        var shellRemovingResult = new ShellRemovingResult
        {
            TenantName = tenant,
        };

        string message = null;
        if (!_shellHost.TryGetSettings(tenant, out var shellSettings))
        {
            message = $"The tenant '{tenant}' doesn't exist.";
        }
        else if (shellSettings.Name == ShellHelper.DefaultShellName)
        {
            message = "The tenant should not be the 'Default' tenant.";
        }
        else if (shellSettings.State != TenantState.Disabled)
        {
            message = $"The tenant '{tenant}' should be 'Disabled'.";
        }

        if (message != null)
        {
            var ex = new InvalidOperationException(message);
            _logger.LogError(ex, "Failed to remove the shell settings of tenant '{TenantName}'.", tenant);
            shellRemovingResult.ErrorMessage = message;
            return shellRemovingResult;
        }

        try
        {
            await _shellHost.RemoveShellSettingsAsync(shellSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the shell settings of tenant '{TenantName}'.",
                tenant);

            shellRemovingResult.ErrorMessage = $"Failed to remove the shell settings of tenant '{tenant}'.";
        }

        return shellRemovingResult;
    }
}
