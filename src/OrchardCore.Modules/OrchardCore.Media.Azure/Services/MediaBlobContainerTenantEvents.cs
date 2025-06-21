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

public sealed class MediaBlobContainerTenantEvents : ModularTenantEvents
{
    private readonly MediaBlobStorageOptions _options;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    internal readonly IStringLocalizer S;

    public MediaBlobContainerTenantEvents(
        IOptions<MediaBlobStorageOptions> options,
        ShellSettings shellSettings,
        IStringLocalizer<MediaBlobContainerTenantEvents> localizer,
        ILogger<MediaBlobContainerTenantEvents> logger
        )
    {
        _options = options.Value;
        _shellSettings = shellSettings;
        S = localizer;
        _logger = logger;
    }

    public override async Task ActivatingAsync()
    {
        // Only create container if options are valid.
        if (_shellSettings.IsUninitialized() || !_options.IsConfigured() || !_options.CreateContainer)
        {
            return;
        }

        _logger.LogDebug("Testing Azure Media Storage container {ContainerName} existence", _options.ContainerName);

        try
        {
            var blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
            var response = await blobContainer.CreateIfNotExistsAsync(PublicAccessType.None).ConfigureAwait(false);

            _logger.LogDebug("Azure Media Storage container {ContainerName} created.", _options.ContainerName);
        }
        catch (RequestFailedException ex)
        {
            _logger.LogError(ex, "Unable to create Azure Media Storage Container.");
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
                var response = await blobContainer.DeleteIfExistsAsync().ConfigureAwait(false);
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
                await foreach (var blobItem in blobContainer.GetBlobsAsync(prefix: _options.BasePath).ConfigureAwait(false))
                {
                    var response = await blobContainer.DeleteBlobIfExistsAsync(blobItem.Name).ConfigureAwait(false);
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
