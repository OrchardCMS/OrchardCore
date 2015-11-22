using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Orchard.FileSystem.AppData;
using Microsoft.Extensions.Logging;
using Microsoft.Dnx.Compilation.Caching;
using Orchard.Parser;
using Orchard.Parser.Yaml;

namespace Orchard.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IAppDataFolder _appDataFolder;
        //private readonly ICache _cache;
        private readonly ILogger _logger;

        private const string SettingsFileNameFormat = "Settings.{0}";

        public ShellSettingsManager(IAppDataFolder appDataFolder,
            //ICache cache,
            ILoggerFactory loggerFactory)
        {
            _appDataFolder = appDataFolder;
            //_cache = cache;
            _logger = loggerFactory.CreateLogger<ShellSettingsManager>();
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings()
        {
            var shellSettings = new List<ShellSettings>();

            foreach (var tenant in _appDataFolder.ListDirectories("Sites"))
            {
                _logger.LogInformation("ShellSettings found in '{0}', attempting to load.", tenant.Name);

                var configurationContainer =
                    new ConfigurationBuilder()
                        .AddJsonFile(_appDataFolder.Combine(tenant.PhysicalPath, string.Format(SettingsFileNameFormat, "json")),
                            true)
                        .AddXmlFile(_appDataFolder.Combine(tenant.PhysicalPath, string.Format(SettingsFileNameFormat, "xml")),
                            true)
                        .AddYamlFile(_appDataFolder.Combine(tenant.PhysicalPath, string.Format(SettingsFileNameFormat, "txt")),
                            false);

                var config = configurationContainer.Build();

                var shellSetting = new ShellSettings(config);
                shellSettings.Add(shellSetting);

                _logger.LogInformation("Loaded ShellSettings for tenant '{0}'", shellSetting.Name);
            }

            return shellSettings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSettings)
        {
            if (shellSettings == null)
                throw new ArgumentNullException(nameof(shellSettings));
            if (string.IsNullOrWhiteSpace(shellSettings.Name))
                throw new ArgumentException(
                    "The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.",
                    nameof(shellSettings.Name));

            _logger.LogInformation("Saving ShellSettings for tenant '{0}'", shellSettings.Name);

            var tenantPath = _appDataFolder.MapPath(_appDataFolder.Combine("Sites", shellSettings.Name));

            var configurationProvider = new YamlConfigurationProvider(
                _appDataFolder.Combine(tenantPath, string.Format(SettingsFileNameFormat, "txt")), false);

            foreach (var key in shellSettings.RootConfiguration.GetChildren())
            {
                configurationProvider.Set(key.Key, key.Value);
            }

            configurationProvider.Commit();

            _logger.LogInformation("Saved ShellSettings for tenant '{0}'", shellSettings.Name);
        }
    }
}