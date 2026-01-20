using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;
using OrchardCore.Liquid.Abstractions;

namespace OrchardCore.Media.AmazonS3.Services;

internal sealed class AwsStorageOptionsConfiguration : IConfigureOptions<AwsStorageOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public AwsStorageOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<AwsStorageOptionsConfiguration> logger)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(AwsStorageOptions options)
    {
        options.BindConfiguration(AmazonS3Constants.ConfigSections.AmazonS3, _shellConfiguration, _logger);

        var parser = new FluidOptionsParser<AwsStorageOptions>(_shellSettings);

        try
        {
            options.BucketName = parser.ParseAndFormat(options.BucketName);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 Media Storage bucket name.");
        }

        try
        {
            options.BasePath = parser.ParseAndFormat(options.BasePath);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 Media Storage base path.");
        }
    }
}
