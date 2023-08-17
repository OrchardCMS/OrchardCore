using System;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Removing;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobContainerTenantEvents : ModularTenantEvents
    {
        private readonly MediaBlobStorageOptions _options;
        private readonly ShellSettings _shellSettings;
        protected readonly IStringLocalizer S;
        private readonly ILogger _logger;

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
            if (_shellSettings.IsUninitialized() ||
                String.IsNullOrEmpty(_options.ConnectionString) ||
                String.IsNullOrEmpty(_options.ContainerName) ||
                !_options.CreateContainer
                )
            {
                return;
            }

            _logger.LogDebug("Testing Azure Media Storage container {ContainerName} existence", _options.ContainerName);

            try
            {
                var _blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
                var response = await _blobContainer.CreateIfNotExistsAsync(PublicAccessType.None);

                _logger.LogDebug("Azure Media Storage container {ContainerName} created.", _options.ContainerName);
            }
            catch (RequestFailedException ex)
            {
                _logger.LogError(ex, "Unable to create Azure Media Storage Container.");
            }
        }

        public override async Task RemovingAsync(ShellRemovingContext context)
        {
            // Only remove container if options are valid.
            if (!_options.RemoveContainer ||
                String.IsNullOrEmpty(_options.ConnectionString) ||
                String.IsNullOrEmpty(_options.ContainerName))
            {
                return;
            }

            try
            {
                var _blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);

                var response = await _blobContainer.DeleteIfExistsAsync();
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
        }
    }
}
