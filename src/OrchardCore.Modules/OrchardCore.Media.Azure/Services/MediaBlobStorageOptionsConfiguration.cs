using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Azure.Helpers;

namespace OrchardCore.Media.Azure.Services
{
    internal class MediaBlobStorageOptionsConfiguration : IConfigureOptions<MediaBlobStorageOptions>
    {
        private readonly IShellConfiguration _shellConfiguration;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;
        private readonly FluidParserHelper<MediaBlobStorageOptionsConfiguration> _fluidParserHelper;

        public MediaBlobStorageOptionsConfiguration(
            IShellConfiguration shellConfiguration,
            ShellSettings shellSettings,
            ILogger<MediaBlobStorageOptionsConfiguration> logger
        )
        {
            _shellConfiguration = shellConfiguration;
            _shellSettings = shellSettings;
            _logger = logger;
            _fluidParserHelper = new(shellSettings);
        }

        public void Configure(MediaBlobStorageOptions options)
        {
            var section = _shellConfiguration.GetSection("OrchardCore_Media_Azure");

            options.BasePath = section.GetValue(nameof(options.BasePath), string.Empty);
            options.ContainerName = section.GetValue(nameof(options.ContainerName), string.Empty);
            options.ConnectionString = section.GetValue(nameof(options.ConnectionString), string.Empty);
            options.CreateContainer = section.GetValue(nameof(options.CreateContainer), true);
            options.RemoveContainer = section.GetValue(nameof(options.RemoveContainer), false);

            try
            {
                // Container name must be lowercase.
                options.ContainerName = _fluidParserHelper.ParseAndFormat(options.ContainerName).ToLower();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse Azure Media Storage container name.");
                throw;
            }

            try
            {
                options.BasePath = _fluidParserHelper.ParseAndFormat(options.BasePath);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse Azure Media Storage base path.");
                throw;
            }
        }
    }
}
