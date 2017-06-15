using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Parser;
using Orchard.Parser.Yaml;

namespace Orchard.Environment.Shell
{
    public class ShellSettingsConfigurationProvider : IShellSettingsConfigurationProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IOptions<ShellOptions> _optionsAccessor;
        private readonly ILogger _logger;
        
        public ShellSettingsConfigurationProvider(
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> optionsAccessor,
            ILogger<ShellSettingsManager> logger)
        {
            _hostingEnvironment = hostingEnvironment;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        public void AddSource(IConfigurationBuilder builder)
        {
            foreach (var tenant in
                _hostingEnvironment.ContentRootFileProvider.GetDirectoryContents(
                    Path.Combine(
                        _optionsAccessor.Value.ShellsRootContainerName,
                        _optionsAccessor.Value.ShellsContainerName)))
            {
                builder
                    .AddYamlFile(ObtainSettingsPath(tenant.PhysicalPath));
            }
        }

        public void SaveToSource(string name, IDictionary<string, string> configuration)
        {
            var settingsFile = ObtainSettingsPath(Path.Combine(
                        _optionsAccessor.Value.ShellsRootContainerName,
                        _optionsAccessor.Value.ShellsContainerName,
                        name));

            var configurationProvider = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = settingsFile,
                Optional = false
            });

            foreach (var key in configuration.Keys)
            {
                configurationProvider.Set(key, configuration[key]);
            }

            configurationProvider.Commit();
        }

        private string ObtainSettingsPath(string tenantPath) => Path.Combine(tenantPath, "Settings.txt");
    }
}
