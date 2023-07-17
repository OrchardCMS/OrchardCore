using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove site folder of a given tenant.
/// </summary>
public class ShellSiteFolderRemovingHandler : IShellRemovingHandler
{
    private readonly ShellOptions _shellOptions;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ShellSiteFolderRemovingHandler(
        IOptions<ShellOptions> shellOptions,
        IStringLocalizer<ShellSiteFolderRemovingHandler> localizer,
        ILogger<ShellSiteFolderRemovingHandler> logger)
    {
        _shellOptions = shellOptions.Value;
        S = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Removes the site folder of the provided tenant.
    /// </summary>
    public Task RemovingAsync(ShellRemovingContext context)
    {
        var shellAppDataFolder = Path.Combine(
            _shellOptions.ShellsApplicationDataPath,
            _shellOptions.ShellsContainerName,
            context.ShellSettings.Name);

        try
        {
            Directory.Delete(shellAppDataFolder, true);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException)
        {
        }
        catch (Exception ex) when (ex.IsFileSharingViolation())
        {
            // Sharing violation, may happen if multiple nodes share the same file system
            // without using a distributed lock, in that case let another node do the job.
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    ex,
@"Sharing violation while removing the site folder '{TenantFolder}' of tenant '{TenantName}'.
Sharing violation may happen if multiple nodes share the same file system without using a distributed lock.
In that case let another node do the job.",
                    shellAppDataFolder,
                    context.ShellSettings.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the site folder '{TenantFolder}' of tenant '{TenantName}'.",
                shellAppDataFolder,
                context.ShellSettings.Name);

            context.ErrorMessage = S["Failed to remove the site folder '{0}'.", shellAppDataFolder];
            context.Error = ex;
        }

        return Task.CompletedTask;
    }
}
