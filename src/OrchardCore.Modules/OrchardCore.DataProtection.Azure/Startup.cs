using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        public Startup(
            IConfiguration configuration, 
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings)
        {
            _configuration = configuration;
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var blobName = $"{_shellOptions.Value.ShellsContainerName}/{_shellSettings.Name}/DataProtectionKeys.xml";

            // Re-register the data protection services to be tenant-aware so that modules that internally
            // rely on IDataProtector/IDataProtectionProvider automatically get an isolated instance that
            // manages its own key ring and doesn't allow decrypting payloads encrypted by another tenant.
            services.Add(new ServiceCollection()
                .AddDataProtection()
                .PersistKeysToAzureBlobStorage(GetBlobContainer(), blobName)
                .SetApplicationName(_shellSettings.Name)
                .Services);
        }

        private CloudBlobContainer GetBlobContainer()
        {
            var connectionString = _configuration.GetValue<string>("Modules:OrchardCore.DataProtection.Azure:ConnectionString");
            var containerName = _configuration.GetValue<string>("Modules:OrchardCore.DataProtection.Azure:ContainerName");

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, new BlobRequestOptions(), new OperationContext()).GetAwaiter().GetResult();

            return blobContainer;
        }
    }
}
