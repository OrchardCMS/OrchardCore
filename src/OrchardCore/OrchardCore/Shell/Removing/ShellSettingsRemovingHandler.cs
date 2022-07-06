using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell.Removing;

public class ShellSettingsRemovingHandler : IShellRemovingHandler
{
    private readonly IShellHost _shellHost;
    private readonly ILogger _logger;

    public ShellSettingsRemovingHandler(
        IShellHost shellHost,
        IOptions<ShellOptions> shellOptions,
        ILogger<ShellSettingsRemovingHandler> logger)
    {
        _shellHost = shellHost;
        _logger = logger;
    }

    public async Task RemovingAsync(ShellRemovingContext context)
    {
        if (!_shellHost.TryGetSettings(context.TenantName, out var shellSettings))
        {
            context.ErrorMessage = $"The tenant '{context.TenantName}' doesn't exist.";
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
                context.TenantName);

            context.ErrorMessage = $"Failed to remove the shell settings of tenant '{context.TenantName}'.";
        }
    }
}
