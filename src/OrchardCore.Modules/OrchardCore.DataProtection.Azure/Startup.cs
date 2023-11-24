using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fluid;
using Microsoft.AspNetCore.DataProtection;
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
        private readonly ILogger _logger;

        // Local instance since it can be discarded once the startup is over.
        private readonly FluidParser _fluidParser = new();

        public Startup(IShellConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            var options = new BlobOptions();

            _configuration.Bind("OrchardCore_DataProtection_Azure", options);
            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                services
                    .AddSingleton(options)
                    .AddDataProtection()
                    .PersistKeysToAzureBlobStorage(sp =>
                    {
                        var options = sp.GetRequiredService<BlobOptions>();
                        return new BlobClient(
                            options.ConnectionString,
                            options.ContainerName,
                            options.BlobName);
                    });

                services.Initialize(async sp =>
                {
                    var blobOptions = sp.GetRequiredService<BlobOptions>();
                    var shellOptions = sp.GetRequiredService<IOptions<ShellOptions>>().Value;
                    var shellSettings = sp.GetRequiredService<ShellSettings>();

                    var fluidParser = new FluidParser();
                    var logger = sp.GetRequiredService<ILogger<Startup>>();

                    await ConfigureContainerNameAsync(blobOptions, shellSettings, fluidParser, logger);
                    await ConfigureBlobNameAsync(blobOptions, shellOptions, shellSettings, fluidParser, logger);
                });
            }
            else
            {
                _logger.LogCritical("No connection string was supplied for OrchardCore.DataProtection.Azure. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
            }
        }

        private static async Task ConfigureContainerNameAsync(
            BlobOptions blobOptions,
            ShellSettings shellSettings,
            FluidParser fluidParser,
            ILogger logger)
        {
            try
            {
                // Use Fluid directly as the service provider has not been built.
                var templateOptions = new TemplateOptions();
                templateOptions.MemberAccessStrategy.Register<ShellSettings>();
                var templateContext = new TemplateContext(templateOptions);
                templateContext.SetValue("ShellSettings", shellSettings);

                var template = fluidParser.Parse(blobOptions.ContainerName);

                // container name must be lowercase
                var containerName = template.Render(templateContext, NullEncoder.Default).ToLower();
                blobOptions.ContainerName = containerName.Replace("\r", string.Empty).Replace("\n", string.Empty);
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Unable to parse data protection connection string.");
                throw;
            }

            if (blobOptions.CreateContainer)
            {
                try
                {
                    logger.LogDebug("Testing data protection container {ContainerName} existence", blobOptions.ContainerName);
                    var blobContainer = new BlobContainerClient(blobOptions.ConnectionString, blobOptions.ContainerName);
                    var response = await blobContainer.CreateIfNotExistsAsync(PublicAccessType.None);
                    logger.LogDebug("Data protection container {ContainerName} created.", blobOptions.ContainerName);
                }
                catch (Exception)
                {
                    logger.LogCritical("Unable to connect to Azure Storage to configure data protection storage. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
                    throw;
                }
            }
        }

        private static async Task ConfigureBlobNameAsync(
            BlobOptions blobOptions,
            ShellOptions shellOptions,
            ShellSettings shellSettings,
            FluidParser fluidParser,
            ILogger logger)
        {
            if (string.IsNullOrEmpty(blobOptions.BlobName))
            {
                blobOptions.BlobName = $"{shellOptions.ShellsContainerName}/{shellSettings.Name}/DataProtectionKeys.xml";
            }
            else
            {
                try
                {
                    // Use Fluid directly as the service provider has not been built.
                    var templateOptions = new TemplateOptions();
                    var templateContext = new TemplateContext(templateOptions);
                    templateOptions.MemberAccessStrategy.Register<ShellSettings>();
                    templateContext.SetValue("ShellSettings", shellSettings);

                    var template = fluidParser.Parse(blobOptions.BlobName);

                    var blobName = await template.RenderAsync(templateContext, NullEncoder.Default);
                    blobOptions.BlobName = blobName.Replace("\r", string.Empty).Replace("\n", string.Empty);
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Unable to parse data protection blob name.");
                    throw;
                }
            }
        }

        // Assume that this module will override default configuration, so set the Order to a value above the default.
        public override int Order => 10;
    }
}
