using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the web root folder of a given tenant.
/// </summary>
public class ShellWebRootRemovingHandler : ShellRemovingHostHandler
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger _logger;

    public ShellWebRootRemovingHandler(
        IWebHostEnvironment webHostEnvironment,
        ILogger<ShellWebRootRemovingHandler> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// Removes the web root folder of the provided tenant.
    /// </summary>
    public override Task RemovingAsync(ShellRemovingContext context)
    {
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

    /// <summary>
    /// In a distributed environment, removes locally the folder of the provided tenant.
    /// </summary>
    public override Task LocalRemovingAsync(ShellRemovingContext context) => RemovingAsync(context);
}
