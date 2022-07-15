using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the web root folder of a given tenant.
/// </summary>
public class ShellWebRootFolderRemovingHandler : IShellRemovingHostHandler
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger _logger;

    public ShellWebRootFolderRemovingHandler(
        IWebHostEnvironment webHostEnvironment,
        ILogger<ShellWebRootFolderRemovingHandler> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// Removes the web root folder of the provided tenant.
    /// </summary>
    public Task RemovingAsync(ShellRemovingContext context)
    {
        if (context.ShellSettings.State == TenantState.Uninitialized)
        {
            return Task.CompletedTask;
        }

        var shellWebRootFolder = Path.Combine(
            _webHostEnvironment.WebRootPath,
            context.ShellSettings.Name);

        try
        {
            Directory.Delete(shellWebRootFolder, true);
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the web root folder '{TenantFolder}' of tenant '{TenantName}'.",
                shellWebRootFolder,
                context.ShellSettings.Name);

            context.ErrorMessage = $"Failed to remove the web root folder '{shellWebRootFolder}'.";
            context.Error = ex;
        }

        return Task.CompletedTask;
    }
}
