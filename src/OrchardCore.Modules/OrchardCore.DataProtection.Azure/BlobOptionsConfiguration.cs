using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Liquid.Abstractions;

namespace OrchardCore.DataProtection.Azure;

internal sealed class BlobOptionsConfiguration : IConfigureOptions<BlobOptions>
{
    private readonly IShellConfiguration _configuration;
    private readonly FluidParser _fluidParser;
    private readonly ShellOptions _shellOptions;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public BlobOptionsConfiguration(
        IShellConfiguration configuration,
        FluidParser fluidParser,
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings,
        ILogger<BlobOptionsConfiguration> logger)
    {
        _configuration = configuration;
        _fluidParser = fluidParser;
        _shellOptions = shellOptions.Value;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(BlobOptions options)
    {
        _logger.LogDebug("Configuring BlobOptions in BlobOptionsSetup");

        _configuration.Bind("OrchardCore_DataProtection_Azure", options);
        ConfigureContainerName(options);
        ConfigureBlobName(options);
    }

    private void ConfigureContainerName(BlobOptions options)
    {
        _logger.LogDebug("Configuring BlobOptions.ContainerName in BlobOptionsSetup");

        try
        {
            // Container name must be lowercase.
            options.ContainerName = new FluidOptionsParser<BlobOptions>(_fluidParser, _shellSettings)
                .ParseAndFormat(options.ContainerName)
                .ToLowerInvariant();

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("BlobOptions.ContainerName was set to {ContainerName}", options.ContainerName);
            }
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
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Testing data protection container {ContainerName} existence", options.ContainerName);
                }
                var blobContainer = new BlobContainerClient(options.ConnectionString, options.ContainerName);
                var response = blobContainer.CreateIfNotExistsAsync(PublicAccessType.None).GetAwaiter().GetResult();
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Data protection container {ContainerName} created.", options.ContainerName);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to connect to Azure Storage to configure data protection storage. Ensure that an application setting containing a valid Azure Storage connection string is available at `Modules:OrchardCore.DataProtection.Azure:ConnectionString`.");
                throw;
            }
        }
    }

    private void ConfigureBlobName(BlobOptions options)
    {
        _logger.LogDebug("Configuring BlobOptions.BlobName in BlobOptionsSetup");

        if (string.IsNullOrEmpty(options.BlobName))
        {
            options.BlobName = $"{_shellOptions.ShellsContainerName}/{_shellSettings.Name}/DataProtectionKeys.xml";

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("BlobOptions.BlobName was set to {BlobName}", options.BlobName);
            }

            return;
        }

        try
        {
            options.BlobName = new FluidOptionsParser<BlobOptions>(_fluidParser, _shellSettings)
                .ParseAndFormat(options.BlobName);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("BlobOptions.BlobName was set to {BlobName}", options.BlobName);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse data protection blob name.");
            throw;
        }
    }
}
