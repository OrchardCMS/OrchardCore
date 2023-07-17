using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell.Removing;

/// <summary>
/// Allows to remove the web root folder of a given tenant.
/// </summary>
public class ShellWebRootRemovingHandler : IShellRemovingHandler
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    protected readonly IStringLocalizer S;
    private readonly ILogger _logger;

    public ShellWebRootRemovingHandler(
        IWebHostEnvironment webHostEnvironment,
        IStringLocalizer<ShellWebRootRemovingHandler> localizer,
        ILogger<ShellWebRootRemovingHandler> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        S = localizer;
        _logger = logger;
    }

    /// <summary>
    /// Removes the web root folder of the provided tenant.
    /// </summary>
    public Task RemovingAsync(ShellRemovingContext context)
    {
        if (context.ShellSettings.IsUninitialized())
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
        catch (Exception ex) when (ex.IsFileSharingViolation())
        {
            // Sharing violation, may happen if multiple nodes share the same file system
            // without using a distributed lock, in that case let another node do the job.
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(
                    ex,
@"Sharing violation while removing the web root folder '{TenantFolder}' of tenant '{TenantName}'.
Sharing violation may happen if multiple nodes share the same file system without using a distributed lock.
In that case let another node do the job.",
                    shellWebRootFolder,
                    context.ShellSettings.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to remove the web root folder '{TenantFolder}' of tenant '{TenantName}'.",
                shellWebRootFolder,
                context.ShellSettings.Name);

            context.ErrorMessage = S["Failed to remove the web root folder '{0}'.", shellWebRootFolder];
            context.Error = ex;
        }

        return Task.CompletedTask;
    }
}
