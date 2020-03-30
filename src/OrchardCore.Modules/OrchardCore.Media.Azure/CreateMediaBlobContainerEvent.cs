using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.Media.Azure
{
    public class CreateMediaBlobContainerEvent : ModularTenantEvents
    {
        private readonly MediaBlobStorageOptions _options;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public CreateMediaBlobContainerEvent(
            IOptions<MediaBlobStorageOptions> options,
            ShellSettings shellSettings,
            ILogger<CreateMediaBlobContainerEvent> logger
            )
        {
            _options = options.Value;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override async Task ActivatingAsync()
        {
            // Only create container if options are valid.

            if (_shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized &&
                !String.IsNullOrEmpty(_options.ConnectionString) &&
                !String.IsNullOrEmpty(_options.ContainerName) &&
                _options.CreateContainer
                )
            {
                _logger.LogDebug("Testing Azure Media Storage container {ContainerName} existence", _options.ContainerName);

                try
                {
                    var _blobContainer = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
                    var response = await _blobContainer.CreateIfNotExistsAsync(PublicAccessType.None);

                    _logger.LogDebug("Azure Media Storage container {ContainerName} created.", _options.ContainerName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to create Azure Media Storage Container.");
                }
            }
        }
    }
}
