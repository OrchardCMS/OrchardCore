using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OrchardCore.Environment.Shell.Removing;

public class SiteFolderRemovingHandler : IShellRemovingHandler
{
    private readonly ShellOptions _shellOptions;
    private readonly ILogger _logger;

    public SiteFolderRemovingHandler(
        IOptions<ShellOptions> shellOptions,
        ILogger<ShellSettingsRemovingHandler> logger)
    {
        _shellOptions = shellOptions.Value;
        _logger = logger;
    }

    public Task<ShellRemovingResult> RemovingAsync(string tenant)
    {
        var shellRemovingResult = new ShellRemovingResult
        {
            TenantName = tenant,
        };

        string message = null;
        if (tenant == ShellHelper.DefaultShellName)
        {
            message = "The tenant should not be the 'Default' tenant.";
        }

        if (message != null)
        {
            var ex = new InvalidOperationException(message);
            _logger.LogError(ex, "Failed to remove the site folder while removing the tenant '{TenantName}'.", tenant);
            shellRemovingResult.ErrorMessage = message;
            return Task.FromResult(shellRemovingResult);
        }

        var tenantFolder = Path.Combine(
            _shellOptions.ShellsApplicationDataPath,
            _shellOptions.ShellsContainerName, tenant);

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
                tenant);

            shellRemovingResult.ErrorMessage = $"Failed to remove the site folder '{tenantFolder}'.";
        }

        return Task.FromResult(shellRemovingResult);
    }
}
