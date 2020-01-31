using System;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
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
                    var storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var blobContainer = blobClient.GetContainerReference(_options.ContainerName);
                    var result = await blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, new BlobRequestOptions(), new OperationContext());

                    _logger.LogDebug("Azure Media Storage container {ContainerName} created: {Result}.", _options.ContainerName, result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to create Azure Media Storage Container.");
                }
            }
        }
    }
}
