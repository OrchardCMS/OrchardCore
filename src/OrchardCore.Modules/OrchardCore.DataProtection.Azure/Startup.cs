using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;

namespace OrchardCore.DataProtection.Azure
{
    public class Startup : StartupBase
    {
        private readonly IConfiguration _configuration;
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger<Startup> _logger;

        public Startup(
            IConfiguration configuration, 
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings, 
            ILogger<Startup> logger)
        {
            _configuration = configuration;
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var blobName = $"{_shellOptions.Value.ShellsContainerName}/{_shellSettings.Name}/DataProtectionKeys.xml";

            services.AddDataProtection().PersistKeysToAzureBlobStorage(GetBlobContainer(), blobName);
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var connectionString = _configuration.GetValue<string>("Modules:OrchardCore.DataProtection.Azure:ConnectionString");
            var containerName = _configuration.GetValue<string>("Modules:OrchardCore.DataProtection.Azure:ContainerName") ?? "dataprotection";

            if (String.IsNullOrWhiteSpace(connectionString))
            {
                _logger.LogCritical("No connection string was supplied for OrchardCore.DataProtection.Azure. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
            }

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();

                var blobContainer = blobClient.GetContainerReference(containerName);
                blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, new BlobRequestOptions(), new OperationContext()).GetAwaiter().GetResult();

                return blobContainer;
            }
            catch (Exception)
            {
                _logger.LogCritical("Unable to connect to Azure Storage to configure data protection storage. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");

                throw;
            }
        }

        // Assume that this module will override default configuration, so set the Order to a value above the default.
        public override int Order => 10;
    }
}
