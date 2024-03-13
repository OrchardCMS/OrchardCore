using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the shell settings of a given tenant.
/// </summary>
public class ShellSettingsRemovingHandler : IShellRemovingHandler
{
    private readonly IShellHost _shellHost;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ShellSettingsRemovingHandler(
        IShellHost shellHost,
        IStringLocalizer<ShellSettingsRemovingHandler> localizer,
        ILogger<ShellSettingsRemovingHandler> logger)
    {
        _shellHost = shellHost;
        S = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Removes the shell settings of the provided tenant.
    /// </summary>
    public async Task RemovingAsync(ShellRemovingContext context)
    {
        try
        {
            if (context.LocalResourcesOnly)
            {
                return;
            }

            await _shellHost.RemoveShellSettingsAsync(context.ShellSettings);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the shell settings of tenant '{TenantName}'.",
                context.ShellSettings.Name);

            context.ErrorMessage = S["Failed to remove the shell settings."];
            context.Error = ex;
        }
    }
}
