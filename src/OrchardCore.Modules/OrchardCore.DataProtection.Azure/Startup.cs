using System;
using Fluid;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.DataProtection.Azure
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _configuration;
        private readonly ShellOptions _shellOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public Startup(
            IShellConfiguration configuration,
            IOptions<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ILogger<Startup> logger)
        {
            _configuration = configuration;
            _shellOptions = shellOptions.Value;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _configuration.GetValue<string>("OrchardCore_DataProtection_Azure:ConnectionString");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                services.AddDataProtection().PersistKeysToAzureBlobStorage(GetBlobContainer(connectionString), GetBlobName());
            }
            else
            {
                _logger.LogCritical("No connection string was supplied for OrchardCore.DataProtection.Azure. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
            }
        }

        private CloudBlobContainer GetBlobContainer(string connectionString)
        {
            var containerName = _configuration.GetValue("OrchardCore_DataProtection_Azure:ContainerName", "dataprotection");

            // Use Fluid directly as the service provider has not been built.
            try
            {
                var templateContext = new TemplateContext();
                templateContext.MemberAccessStrategy.Register<ShellSettings>();
                templateContext.SetValue("ShellSettings", _shellSettings);

                var template = FluidTemplate.Parse(containerName);

                // container name must be lowercase
                containerName = template.Render(templateContext, NullEncoder.Default).ToLower();
                containerName.Replace("\r", String.Empty).Replace("\n", String.Empty);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse data protection connection string.");
                throw e;
            }

            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var blobContainer = blobClient.GetContainerReference(containerName);
                var createContainer = _configuration.GetValue("OrchardCore_DataProtection_Azure:CreateContainer", true);

                if (createContainer)
                {
                    _logger.LogDebug("Testing data protection container {ContainerName} existence", containerName);
                    var result = blobContainer.CreateIfNotExistsAsync(BlobContainerPublicAccessType.Off, new BlobRequestOptions(), new OperationContext()).GetAwaiter().GetResult();
                    _logger.LogDebug("Data protection container {ContainerName} created: {Result}.", containerName, result);
                }

                return blobContainer;
            }
            catch (Exception)
            {
                _logger.LogCritical("Unable to connect to Azure Storage to configure data protection storage. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");

                throw;
            }
        }

        private string GetBlobName()
        {
            var blobName = _configuration.GetValue<string>("OrchardCore_DataProtection_Azure:BlobName");

            if (String.IsNullOrEmpty(blobName))
            {
                blobName = $"{_shellOptions.ShellsContainerName}/{_shellSettings.Name}/DataProtectionKeys.xml";
            }
            else
            {
                try
                {
                    // Use Fluid directly as the service provider has not been built.
                    var templateContext = new TemplateContext();
                    templateContext.MemberAccessStrategy.Register<ShellSettings>();
                    templateContext.SetValue("ShellSettings", _shellSettings);

                    var template = FluidTemplate.Parse(blobName);

                    blobName = template.Render(templateContext, NullEncoder.Default);
                    blobName.Replace("\r", String.Empty).Replace("\n", String.Empty);
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, "Unable to parse data protection blob name.");
                    throw e;
                }
            }

            return blobName;
        }

        // Assume that this module will override default configuration, so set the Order to a value above the default.
        public override int Order => 10;
    }
}
