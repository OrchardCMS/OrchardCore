using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Orchard.FileSystem.AppData;
using Orchard.Parser;
using Orchard.Parser.Yaml;
using Orchard.Validation;
using System.Collections.Generic;

namespace Orchard.Environment.Shell
{
    public class ShellSettingsManager : IShellSettingsManager
    {
        private readonly IAppDataFolder _appDataFolder;
        //private readonly ICache _cache;
        private readonly IOptions<ShellOptions> _optionsAccessor;
        private readonly ILogger _logger;

        private const string SettingsFileNameFormat = "Settings.{0}";

        public ShellSettingsManager(IAppDataFolder appDataFolder,
            //ICache cache,
            IOptions<ShellOptions> optionsAccessor,
            ILogger<ShellSettingsManager> logger)
        {
            _appDataFolder = appDataFolder;
            //_cache = cache;
            _optionsAccessor = optionsAccessor;
            _logger = logger;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings()
        {
            var shellSettings = new List<ShellSettings>();

            foreach (var tenant in _appDataFolder.ListDirectories(_optionsAccessor.Value.Location))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("ShellSettings found in '{0}', attempting to load.", tenant.Name);
                }
                var configurationContainer =
                    new ConfigurationBuilder()
                        .AddJsonFile(_appDataFolder.Combine(tenant.PhysicalPath, string.Format(SettingsFileNameFormat, "json")),
                            true)
                        .AddXmlFile(_appDataFolder.Combine(tenant.PhysicalPath, string.Format(SettingsFileNameFormat, "xml")),
                            true)
                        .AddYamlFile(_appDataFolder.Combine(tenant.PhysicalPath, string.Format(SettingsFileNameFormat, "txt")),
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
            var tenantPath = _appDataFolder.MapPath(
                _appDataFolder.Combine(
                    _optionsAccessor.Value.Location, 
                    shellSettings.Name, 
                    string.Format(SettingsFileNameFormat, "txt")));

            var configurationProvider = 
                new YamlConfigurationProvider(tenantPath, false);

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