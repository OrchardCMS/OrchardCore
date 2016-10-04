using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Parser;
using Orchard.Parser.Yaml;
using Orchard.Validation;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IOptions<ShellOptions> _optionsAccessor;
        private readonly ILogger _logger;

        private const string SettingsFileNameFormat = "Settings.{0}";

        public ShellSettingsManager(
            IOptions<ShellOptions> optionsAccessor,
            ILogger<ShellSettingsManager> logger)
        {
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings()
        {
            var shellSettings = new List<ShellSettings>();

            foreach (var tenant in _optionsAccessor.Value.Shells)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("ShellSettings found in '{0}', attempting to load.", tenant.Name);
                }
                var configurationContainer =
                    new ConfigurationBuilder()
                        .SetBasePath(tenant.PhysicalPath)
                        .AddJsonFile(string.Format(SettingsFileNameFormat, "json"),
                            true)
                        .AddXmlFile(string.Format(SettingsFileNameFormat, "xml"),
                            true)
                        .AddYamlFile(string.Format(SettingsFileNameFormat, "txt"),
                            false);

                var config = configurationContainer.Build();
                var shellSetting = ShellSettingsSerializer.ParseSettings(config);
                shellSettings.Add(shellSetting);

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded ShellSettings for tenant '{0}'", shellSetting.Name);
                }
            }

            return shellSettings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSettings)
        {
            Argument.ThrowIfNull(shellSettings, nameof(shellSettings));
            Argument.ThrowIfNullOrWhiteSpace(shellSettings.Name,
                nameof(shellSettings.Name),
                "The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.");

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saving ShellSettings for tenant '{0}'", shellSettings.Name);
            }

            var tenantPath = 
                Path.Combine(
                    _optionsAccessor.Value.ShellContainer.PhysicalPath, 
                    shellSettings.Name, 
                    string.Format(SettingsFileNameFormat, "txt"));

            var configurationProvider = new YamlConfigurationProvider(new YamlConfigurationSource
            {
                Path = tenantPath,
                Optional = false
            });

            foreach (var key in shellSettings.Keys)
            {
                if (!string.IsNullOrEmpty(shellSettings[key]))
                {
                    configurationProvider.Set(key, shellSettings[key]);
                }
            }

            configurationProvider.Commit();

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Saved ShellSettings for tenant '{0}'", shellSettings.Name);
            }
        }
    }
}