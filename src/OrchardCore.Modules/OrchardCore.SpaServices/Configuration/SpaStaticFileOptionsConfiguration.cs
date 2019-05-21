using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.SpaServices.Configuration
{
    public class SpaStaticFileOptionsConfiguration :
        IConfigureOptions<SpaStaticFilesOptions>
    {
        /// <summary>
        /// The path in the tenant's App_Data folder containing the spa static files
        /// </summary>
        public const string SpaStaticFilesPath = "spa";

        private readonly ShellSettings _shellSettings;
        private readonly IOptions<ShellOptions> _shellOptions;
        private readonly ILogger<SpaStaticFileOptionsConfiguration> _logger;

        public SpaStaticFileOptionsConfiguration(
            ShellSettings shellSettings,
            IOptions<ShellOptions> shellOptions,
            ILogger<SpaStaticFileOptionsConfiguration> logger)
        {
            _shellSettings = shellSettings;
            _shellOptions = shellOptions;
            _logger = logger;
        }

        public void Configure(SpaStaticFilesOptions options)
        {
            var rootPath = GetSpaStaticPath(_shellOptions.Value, _shellSettings);
            options.RootPath = rootPath;
            if (!Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            };
        }

        public static string GetSpaStaticPath(ShellOptions shellOptions, ShellSettings shellSettings)
        {
            return Path.Combine(shellOptions.ShellsApplicationDataPath, shellOptions.ShellsContainerName, shellSettings.Name, SpaStaticFilesPath);
        }
    }
}
