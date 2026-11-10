using Fluid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Liquid.Abstractions;

namespace OrchardCore.Media.AmazonS3.Services;

internal sealed class AwsMediaImageCacheOptionsConfiguration : IConfigureOptions<AwsMediaImageCacheOptions>
{
    private readonly FluidParser _fluidParser;
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public AwsMediaImageCacheOptionsConfiguration(
        FluidParser fluidParser,
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<AwsStorageOptionsConfiguration> logger)
    {
        _fluidParser = fluidParser;
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(AwsMediaImageCacheOptions options)
    {
        if (!_shellConfiguration.TryBindConfiguration<AwsMediaImageCacheOptions>(AmazonS3Constants.ConfigSections.AmazonS3ImageCache, AmazonS3Constants.ConfigSections.LegacyAmazonS3ImageCache, _logger, out var rawOptions))
        {
            return;
        }

        options.BucketName = rawOptions.BucketName;
        options.BasePath = rawOptions.BasePath;
        options.CreateBucket = rawOptions.CreateBucket;
        options.RemoveBucket = rawOptions.RemoveBucket;
        options.AwsOptions = rawOptions.AwsOptions;

        var parser = new FluidOptionsParser<AwsMediaImageCacheOptions>(_fluidParser, _shellSettings);

        try
        {
            options.BucketName = parser.ParseAndFormat(options.BucketName);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 Media Image Cache bucket name.");
        }

        try
        {
            options.BasePath = parser.ParseAndFormat(options.BasePath);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 Media Image Cache base path.");
        }
    }
}
