using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Liquid.Abstractions;

namespace OrchardCore.FileStorage.AzureBlob;

public abstract class BlobStorageOptionsConfiguration<TOptions> : IConfigureOptions<TOptions>
    where TOptions : BlobStorageOptions
{
    private readonly ShellSettings _sellSettings;
    private readonly ILogger _logger;

    public BlobStorageOptionsConfiguration(
        ShellSettings sellSettings,
        ILogger logger)
    {
        _sellSettings = sellSettings;
        _logger = logger;
    }

    protected abstract TOptions GetRawOptions();

    public void Configure(TOptions options)
    {
        var rawOptions = GetRawOptions();

        if (rawOptions == null || !rawOptions.IsConfigured())
        {
            return;
        }

        var parser = new FluidOptionsParser<TOptions>(_sellSettings);

        options.ConnectionString = rawOptions.ConnectionString;

        if (!string.IsNullOrEmpty(rawOptions.ContainerName))
        {
            try
            {
                // Container name must be lowercase.
                options.ContainerName = parser.ParseAndFormat(rawOptions.ContainerName).ToLowerInvariant();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse container name for {OptionName}.", typeof(TOptions).Name);
                throw;
            }
        }

        if (!string.IsNullOrWhiteSpace(rawOptions.BasePath))
        {
            try
            {
                options.BasePath = parser.ParseAndFormat(rawOptions.BasePath);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse base path for {OptionName}.", typeof(TOptions).Name);
                throw;
            }
        }

        FurtherConfigure(rawOptions, options);
    }

    /// <summary>
    /// Allows you to configure additional options in an inherited class.
    /// </summary>
    /// <param name="rawOptions">The options as returned by <see cref="GetRawOptions"/></param>
    /// <param name="options">The options to configure.</param>
    protected virtual void FurtherConfigure(TOptions rawOptions, TOptions options)
    {
    }
}
