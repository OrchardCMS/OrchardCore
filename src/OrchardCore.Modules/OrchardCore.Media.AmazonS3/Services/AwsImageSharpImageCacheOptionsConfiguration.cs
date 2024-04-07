using System;
using Fluid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Azure;

namespace OrchardCore.Media.AmazonS3.Services;

public class AwsImageSharpImageCacheOptionsConfiguration : IConfigureOptions<AwsImageSharpImageCacheOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    // Local instance since it can be discarded once the startup is over.
    private readonly FluidParser _fluidParser = new();

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
        options.BindConfiguration(Constants.ConfigSections.AmazonS3ImageSharpCache, _shellConfiguration, _logger);

        var templateOptions = new TemplateOptions();
        var templateContext = new TemplateContext(templateOptions);
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();
        templateOptions.MemberAccessStrategy.Register<AwsImageSharpImageCacheOptions>();
        templateContext.SetValue("ShellSettings", _shellSettings);

        ParseBucketName(options, templateContext);
        ParseBasePath(options, templateContext);
    }

    private void ParseBucketName(AwsImageSharpImageCacheOptions options, TemplateContext templateContext)
    {
        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        try
        {
            var template = _fluidParser.Parse(options.BucketName);

            options.BucketName = template
                .Render(templateContext, NullEncoder.Default)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Trim();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 ImageSharp Image Cache bucket name.");
        }
    }

    private void ParseBasePath(AwsImageSharpImageCacheOptions options, TemplateContext templateContext)
    {
        try
        {
            var template = _fluidParser.Parse(options.BasePath);

            options.BasePath = template
                .Render(templateContext, NullEncoder.Default)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Trim();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Unable to parse Amazon S3 ImageSharp Image Cache base path.");
        }
    }
}
