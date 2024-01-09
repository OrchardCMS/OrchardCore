using System;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Media.Azure.Services;

internal class ImageSharpImageCacheOptionsConfiguration : IConfigureOptions<ImageSharpImageCacheOptions>
{
    private readonly IShellConfiguration _shellConfiguration;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    // Local instance since it can be discarded once the startup is over.
    private readonly FluidParser _fluidParser = new();

    public ImageSharpImageCacheOptionsConfiguration(
        IShellConfiguration shellConfiguration,
        ShellSettings shellSettings,
        ILogger<ImageSharpImageCacheOptionsConfiguration> logger)
    {
        _shellConfiguration = shellConfiguration;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(ImageSharpImageCacheOptions options)
    {
        var section = _shellConfiguration.GetSection("OrchardCore_Media_Azure_ImageSharp_Cache");

        options.ContainerName = section.GetValue(nameof(options.ContainerName), string.Empty);
        options.ConnectionString = section.GetValue(nameof(options.ConnectionString), string.Empty);
        options.CreateContainer = section.GetValue(nameof(options.CreateContainer), true);
        options.RemoveContainer = section.GetValue(nameof(options.RemoveContainer), false);

        var templateOptions = new TemplateOptions();
        var templateContext = new TemplateContext(templateOptions);
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();
        templateOptions.MemberAccessStrategy.Register<ImageSharpImageCacheOptions>();
        templateContext.SetValue("ShellSettings", _shellSettings);

        ParseContainerName(options, templateContext);
    }

    private void ParseContainerName(ImageSharpImageCacheOptions options, TemplateContext templateContext)
    {
        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        try
        {
            var template = _fluidParser.Parse(options.ContainerName);

            // Container name must be lowercase.
            options.ContainerName = template.Render(templateContext, NullEncoder.Default).ToLower();
            options.ContainerName = options.ContainerName.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Unable to parse Azure Media ImageSharp Image Cache container name.");
            throw;
        }
    }
}
