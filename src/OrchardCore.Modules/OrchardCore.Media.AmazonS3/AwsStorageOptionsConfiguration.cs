using System;
using Fluid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.FileStorage.AmazonS3;

namespace OrchardCore.Media.AmazonS3;

public class AwsStorageOptionsConfiguration : IConfigureOptions<AwsStorageOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    // Local instance since it can be discarded once the startup is over.
    private readonly FluidParser _fluidParser = new();

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
        options.BindConfiguration(_shellConfiguration, _logger);

        var templateOptions = new TemplateOptions();
        var templateContext = new TemplateContext(templateOptions);
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();
        templateOptions.MemberAccessStrategy.Register<AwsStorageOptions>();
        templateContext.SetValue("ShellSettings", _shellSettings);

        ParseBucketName(options, templateContext);
        ParseBasePath(options, templateContext);
    }

    private void ParseBucketName(AwsStorageOptions options, TemplateContext templateContext)
    {
        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        try
        {
            var template = _fluidParser.Parse(options.BucketName);

            options.BucketName = template
                .Render(templateContext, NullEncoder.Default)
                .Replace("\r", String.Empty)
                .Replace("\n", String.Empty)
                .Trim();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 Media Storage bucket name.");
            throw;
        }
    }

    private void ParseBasePath(AwsStorageOptions options, TemplateContext templateContext)
    {
        try
        {
            var template = _fluidParser.Parse(options.BasePath);

            options.BasePath = template
                .Render(templateContext, NullEncoder.Default)
                .Replace("\r", String.Empty)
                .Replace("\n", String.Empty)
                .Trim();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 Media Storage base path.");
            throw;
        }
    }
}
