using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.DataProtection.Azure;

public class BlobOptionsSetup : IAsyncConfigureOptions<BlobOptions>
{
    private readonly FluidParser _fluidParser = new();

    private readonly IShellConfiguration _configuration;
    private readonly ShellOptions _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public BlobOptionsSetup(
        IShellConfiguration configuration,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        ILogger<BlobOptionsSetup> logger)
    {
        _configuration = configuration;
        _shellOptions = shellOptions.Value;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public async ValueTask ConfigureAsync(BlobOptions options)
    {
        _configuration.Bind("OrchardCore_DataProtection_Azure", options);
        await ConfigureContainerNameAsync(options);
        await ConfigureBlobNameAsync(options);
    }

    private async ValueTask ConfigureContainerNameAsync(BlobOptions options)
    {
        try
        {
            // Use Fluid directly as the service provider has not been built.
            var templateOptions = new TemplateOptions();
            templateOptions.MemberAccessStrategy.Register<ShellSettings>();
            var templateContext = new TemplateContext(templateOptions);
            templateContext.SetValue("ShellSettings", _shellSettings);

            var template = _fluidParser.Parse(options.ContainerName);

            // Container name must be lowercase.
            var containerName = (await template.RenderAsync(templateContext, NullEncoder.Default)).ToLower();
            options.ContainerName = containerName.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse data protection connection string.");
            throw;
        }

        if (options.CreateContainer)
        {
            try
            {
                _logger.LogDebug("Testing data protection container {ContainerName} existence", options.ContainerName);
                var blobContainer = new BlobContainerClient(options.ConnectionString, options.ContainerName);
                var response = await blobContainer.CreateIfNotExistsAsync(PublicAccessType.None);
                _logger.LogDebug("Data protection container {ContainerName} created.", options.ContainerName);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to connect to Azure Storage to configure data protection storage. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
                throw;
            }
        }
    }

    private async ValueTask ConfigureBlobNameAsync(BlobOptions options)
    {
        if (string.IsNullOrEmpty(options.BlobName))
        {
            options.BlobName = $"{_shellOptions.ShellsContainerName}/{_shellSettings.Name}/DataProtectionKeys.xml";

            return;
        }

        try
        {
            // Use Fluid directly as the service provider has not been built.
            var templateOptions = new TemplateOptions();
            var templateContext = new TemplateContext(templateOptions);
            templateOptions.MemberAccessStrategy.Register<ShellSettings>();
            templateContext.SetValue("ShellSettings", _shellSettings);

            var template = _fluidParser.Parse(options.BlobName);

            var blobName = await template.RenderAsync(templateContext, NullEncoder.Default);
            options.BlobName = blobName.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse data protection blob name.");
            throw;
        }
    }
}
