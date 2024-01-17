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
            section.Bind(options);

            var fluidParserHelper = new FluidParserHelper<MediaBlobStorageOptionsConfiguration>(_shellSettings);

            try
            {
                // Container name must be lowercase.
                options.ContainerName = fluidParserHelper.ParseAndFormat(options.ContainerName).ToLower();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse Azure Media Storage container name.");
                throw;
            }

            try
            {
                options.BasePath = fluidParserHelper.ParseAndFormat(options.BasePath);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Unable to parse Azure Media Storage base path.");
                throw;
            }
        }
    }
}
