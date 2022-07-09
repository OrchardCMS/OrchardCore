using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell.Removing;

public class SiteFolderRemovingHandler : ShellRemovingHostHandler
{
    private readonly ShellOptions _shellOptions;
    private readonly ILogger _logger;

    public SiteFolderRemovingHandler(
        IOptions<ShellOptions> shellOptions,
        ILogger<SiteFolderRemovingHandler> logger)
    {
        _shellOptions = shellOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Removes the folder of the provided tenant.
    /// </summary>
    public override Task RemovingAsync(ShellRemovingContext context)
    {
        var tenantFolder = Path.Combine(
            _shellOptions.ShellsApplicationDataPath,
            _shellOptions.ShellsContainerName, context.ShellSettings.Name);

        try
        {
            Directory.Delete(tenantFolder, true);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the site folder '{tenantFolder}' of tenant '{TenantName}'.",
                tenantFolder,
                context.ShellSettings.Name);

            context.ErrorMessage = $"Failed to remove the site folder '{tenantFolder}'.";
            context.Error = ex;
        }

        return Task.CompletedTask;
    }


    /// <summary>
    /// In a distributed environment, removes locally the folder of the provided tenant.
    /// </summary>
    public override Task LocalRemovingAsync(ShellRemovingContext context) => RemovingAsync(context);
}
