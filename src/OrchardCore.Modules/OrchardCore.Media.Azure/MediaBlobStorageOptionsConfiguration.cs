using System;
using Fluid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Media.Azure
{
    public class MediaBlobStorageOptionsConfiguration : IConfigureOptions<MediaBlobStorageOptions>
    {
        private readonly IShellConfiguration _shellConfiguration;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        // Local instance since it can be discarded once the startup is over
        private readonly FluidParser _fluidParser = new();

        public MediaBlobStorageOptionsConfiguration(
            IShellConfiguration shellConfiguration,
            ShellSettings shellSettings,
            ILogger<MediaBlobStorageOptionsConfiguration> logger
            )
        {
            _shellConfiguration = shellConfiguration;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public void Configure(MediaBlobStorageOptions options)
        {
            var section = _shellConfiguration.GetSection("OrchardCore_Media_Azure");

            options.BasePath = section.GetValue(nameof(options.BasePath), String.Empty);
            options.ContainerName = section.GetValue(nameof(options.ContainerName), String.Empty);
            options.ConnectionString = section.GetValue(nameof(options.ConnectionString), String.Empty);
            options.CreateContainer = section.GetValue(nameof(options.CreateContainer), true);
            options.RemoveContainer = section.GetValue(nameof(options.RemoveContainer), false);

            var templateOptions = new TemplateOptions();
            var templateContext = new TemplateContext(templateOptions);
            templateOptions.MemberAccessStrategy.Register<ShellSettings>();
            templateOptions.MemberAccessStrategy.Register<MediaBlobStorageOptions>();
            templateContext.SetValue("ShellSettings", _shellSettings);

            ParseContainerName(options, templateContext);
            ParseBasePath(options, templateContext);
        }

        private void ParseContainerName(MediaBlobStorageOptions options, TemplateContext templateContext)
        {
            // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
            try
            {
                var template = _fluidParser.Parse(options.ContainerName);

                // container name must be lowercase
                options.ContainerName = template.Render(templateContext, NullEncoder.Default).ToLower();
                options.ContainerName = options.ContainerName.Replace("\r", String.Empty).Replace("\n", String.Empty);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse Azure Media Storage container name.");
                throw;
            }
        }

        private void ParseBasePath(MediaBlobStorageOptions options, TemplateContext templateContext)
        {
            try
            {
                var template = _fluidParser.Parse(options.BasePath);

                options.BasePath = template.Render(templateContext, NullEncoder.Default);
                options.BasePath = options.BasePath.Replace("\r", String.Empty).Replace("\n", String.Empty);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse Azure Media Storage base path.");
                throw;
            }
        }
    }
}
