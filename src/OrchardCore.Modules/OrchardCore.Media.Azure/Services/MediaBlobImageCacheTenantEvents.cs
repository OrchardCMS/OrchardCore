using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure.Services;

internal sealed class MediaBlobImageCacheTenantEvents : ModularTenantEvents
{
    private readonly MediaBlobImageCacheOptions _options;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    private readonly IStringLocalizer S;

    public MediaBlobImageCacheTenantEvents(
        IOptions<MediaBlobImageCacheOptions> options,
        ShellSettings shellSettings,
        ILogger<MediaBlobImageCacheTenantEvents> logger,
        IStringLocalizer<MediaBlobImageCacheTenantEvents> localizer)
    {
        _options = options.Value;
        _shellSettings = shellSettings;
        _logger = logger;
        S = localizer;
    }

    public override async Task ActivatingAsync()
    {
        if (_shellSettings.IsUninitialized() || !_options.IsConfigured() || !_options.CreateContainer)
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Testing Azure Media Image Cache container {ContainerName} existence", _options.ContainerName);
        }

        try
        {
            var blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
            await blobContainer.CreateIfNotExistsAsync(PublicAccessType.None);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Azure Media Image Cache container {ContainerName} created.", _options.ContainerName);
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "InvalidResourceName" || ex.ErrorCode == "OutOfRangeInput")
        {
            _logger.LogError(
                ex,
                "Unable to create Azure Media Image Cache Container '{ContainerName}': the container name is invalid per Azure Blob naming rules. " +
                "See https://learn.microsoft.com/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata.",
                _options.ContainerName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Unable to create Azure Media Image Cache Container '{ContainerName}'.", _options.ContainerName);
        }
    }

    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        if (!_options.RemoveContainer || !_options.IsConfigured())
        {
            return;
        }

        try
        {
            var blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
            var response = await blobContainer.DeleteIfExistsAsync();
            if (!response.Value)
            {
                _logger.LogError("Unable to remove the Azure Media Image Cache Container {ContainerName}.", _options.ContainerName);
                context.ErrorMessage = S["Unable to remove the Azure Media Image Cache Container '{0}'.", _options.ContainerName];
            }
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Failed to remove the Azure Media Image Cache Container {ContainerName}.", _options.ContainerName);
            context.ErrorMessage = S["Failed to remove the Azure Media Image Cache Container '{0}'.", _options.ContainerName];
            context.Error = ex;
        }
    }
}
