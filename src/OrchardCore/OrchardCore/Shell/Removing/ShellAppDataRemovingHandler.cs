using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the app data folder of a given tenant.
/// </summary>
public class ShellAppDataRemovingHandler : ShellRemovingHostHandler
{
    private readonly ShellOptions _shellOptions;
    private readonly ILogger _logger;

    public ShellAppDataRemovingHandler(
        IOptions<ShellOptions> shellOptions,
        ILogger<ShellAppDataRemovingHandler> logger)
    {
        _shellOptions = shellOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Removes the app data folder of the provided tenant.
    /// </summary>
    public override Task RemovingAsync(ShellRemovingContext context)
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
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the app data folder '{TenantFolder}' of tenant '{TenantName}'.",
                shellAppDataFolder,
                context.ShellSettings.Name);

            context.ErrorMessage = $"Failed to remove the app data folder '{shellAppDataFolder}'.";
            context.Error = ex;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// In a distributed environment, removes locally the app data folder of the provided tenant.
    /// </summary>
    public override Task LocalRemovingAsync(ShellRemovingContext context) => RemovingAsync(context);
}
