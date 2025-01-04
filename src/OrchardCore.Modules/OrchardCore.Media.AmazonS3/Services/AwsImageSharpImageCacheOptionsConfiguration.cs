using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Liquid.Abstractions;

namespace OrchardCore.Media.AmazonS3.Services;

internal sealed class AwsImageSharpImageCacheOptionsConfiguration : IConfigureOptions<AwsImageSharpImageCacheOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public AwsImageSharpImageCacheOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<AwsStorageOptionsConfiguration> logger)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(AwsImageSharpImageCacheOptions options)
    {
        options.BindConfiguration(AmazonS3Constants.ConfigSections.AmazonS3ImageSharpCache, _shellConfiguration, _logger);

        var parser = new FluidOptionsParser<AwsImageSharpImageCacheOptions>(_shellSettings);

        try
        {
            options.BucketName = parser.ParseAndFormat(options.BucketName);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 ImageSharp Image Cache bucket name.");
        }

        try
        {
            options.BasePath = parser.ParseAndFormat(options.BasePath);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 ImageSharp Image Cache base path.");
        }
    }
}
