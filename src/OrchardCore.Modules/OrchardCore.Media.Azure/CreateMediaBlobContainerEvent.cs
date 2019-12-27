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
        private readonly IShellConfiguration _shellConfiguration;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public CreateMediaBlobContainerEvent(
            IOptions<MediaBlobStorageOptions> options,
            IShellConfiguration shellConfiguration,
            ShellSettings shellSettings,
            ILogger<CreateMediaBlobContainerEvent> logger
            )
        {
            _options = options.Value;
            _shellConfiguration = shellConfiguration;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override async Task ActivatingAsync()
        {
            // Only create container if options are valid.
            var connectionString = _shellConfiguration[$"OrchardCore.Media.Azure:{nameof(MediaBlobStorageOptions.ConnectionString)}"];
            var containerName = _shellConfiguration[$"OrchardCore.Media.Azure:{nameof(MediaBlobStorageOptions.ContainerName)}"];

            if (_shellSettings.State != Environment.Shell.Models.TenantState.Uninitialized &&
                !String.IsNullOrEmpty(connectionString) &&
                !String.IsNullOrEmpty(containerName)
                )
            {
                _logger.LogDebug("Creating Azure Media Storage Container {ContainerName}", _options.ContainerName);

                try
                {
                    var storageAccount = CloudStorageAccount.Parse(_options.ConnectionString);
                    var blobClient = storageAccount.CreateCloudBlobClient();

                    var blobContainer = blobClient.GetContainerReference(_options.ContainerName);
                    await blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, new BlobRequestOptions(), new OperationContext());

                    _logger.LogDebug("Azure Media Storage Container {ContainerName} created.", _options.ContainerName);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to create Azure Media Storage Container.");
                }
            }
        }
    }
}
