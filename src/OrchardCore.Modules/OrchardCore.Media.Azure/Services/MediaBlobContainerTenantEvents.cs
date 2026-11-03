using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.FileStorage.AzureBlob;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure.Services;

public sealed class MediaBlobContainerTenantEvents : ModularTenantEvents
{
    private readonly MediaBlobStorageOptions _options;
    private readonly ShellSettings _shellSettings;
    private readonly BlobFileStore _blobFileStore;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public MediaBlobContainerTenantEvents(
        IOptions<MediaBlobStorageOptions> options,
        ShellSettings shellSettings,
        BlobFileStore blobFileStore,
        IStringLocalizer<MediaBlobContainerTenantEvents> localizer,
        ILogger<MediaBlobContainerTenantEvents> logger
        )
    {
        _options = options.Value;
        _shellSettings = shellSettings;
        _blobFileStore = blobFileStore;
        S = localizer;
        _logger = logger;
    }

    public override async Task ActivatingAsync()
    {
        try
        {
            await _blobFileStore.EnsureCapabilitiesAsync();
        }
        catch (Exception ex)
        {
            // Don't let invalid HNS configuration take down the whole tenant. Namespace-sensitive
            // operations will surface the configuration error when the media store is used.
            _logger.LogError(
                ex,
                "Unable to initialize the Azure Media Storage account type (Gen1 flat namespace or Gen2 Hierarchical Namespace).");
        }

        // Only create container if options are valid.
        if (_shellSettings.IsUninitialized() || !_options.IsConfigured() || !_options.CreateContainer)
        {
            return;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Testing Azure Media Storage container {ContainerName} existence", _options.ContainerName);
        }

        try
        {
            var blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
            var response = await blobContainer.CreateIfNotExistsAsync(PublicAccessType.None);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Azure Media Storage container {ContainerName} created.", _options.ContainerName);
            }
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "InvalidResourceName" || ex.ErrorCode == "OutOfRangeInput")
        {
            _logger.LogError(
                ex,
                "Unable to create Azure Media Storage Container '{ContainerName}': the container name is invalid per Azure Blob naming rules. " +
                "See https://learn.microsoft.com/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata.",
                _options.ContainerName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Unable to create Azure Media Storage Container '{ContainerName}'.", _options.ContainerName);
        }
    }

    public override async Task RemovingAsync(ShellRemovingContext context)
    {
        // Only carry out removal if options are valid.
        if (!_options.IsConfigured() || (!_options.RemoveFilesFromBasePath && !_options.RemoveContainer))
        {
            return;
        }

        var blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        if (_options.RemoveContainer)
        {
            try
            {
                var response = await blobContainer.DeleteIfExistsAsync();
                if (!response.Value)
                {
                    _logger.LogError("Unable to remove the Azure Media Storage Container {ContainerName}.", _options.ContainerName);

                    context.ErrorMessage = S["Unable to remove the Azure Media Storage Container '{0}'.", _options.ContainerName];
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Failed to remove the Azure Media Storage Container {ContainerName}.", _options.ContainerName);

                context.ErrorMessage = S["Failed to remove the Azure Media Storage Container '{0}'.", _options.ContainerName];
                context.Error = ex;
            }

            // Return here to avoid errors when trying to delete files from a non-existent container in case both deletion options are set.
            return;
        }

        if (_options.RemoveFilesFromBasePath)
        {
            try
            {
                await foreach (var blobItem in blobContainer.GetBlobsAsync(BlobTraits.None, BlobStates.None, _options.BasePath, CancellationToken.None))
                {
                    var response = await blobContainer.DeleteBlobIfExistsAsync(blobItem.Name);
                    if (!response.Value)
                    {
                        _logger.LogError("File removal process failed on file {ItemName}.", blobItem.Name);

                        context.ErrorMessage = S["File removal process failed on file {ItemName}.", blobItem.Name];

                        // Also stop the removal process if a file fails.
                        break;
                    }
                }
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError("Error during Azure Media Storage blob item removal.");

                context.ErrorMessage = S["Error during Azure Media Storage blob item removal."];
                context.Error = ex;
            }
        }
    }
}
