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

    public Task RemovingAsync(ShellRemovingContext context)
    {
        var tenantFolder = Path.Combine(
            _shellOptions.ShellsApplicationDataPath,
            _shellOptions.ShellsContainerName, context.TenantName);

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
                context.TenantName);

            context.ErrorMessage = $"Failed to remove the site folder '{tenantFolder}'.";
        }

        return Task.CompletedTask;
    }
}
