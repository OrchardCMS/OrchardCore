using System;
using System.Collections.Generic;
using Microsoft.Framework.Configuration;
using OrchardVNext.Configuration.Environment.Sources;
using OrchardVNext.FileSystem.AppData;
using System.Linq;

namespace OrchardVNext.Configuration.Environment {
    public interface IShellSettingsManager {
        /// <summary>
        /// Retrieves all shell settings stored.
        /// </summary>
        /// <returns>All shell settings.</returns>
        IEnumerable<ShellSettings> LoadSettings();

        /// <summary>
        /// Persists shell settings to the storage.
        /// </summary>
        /// <param name="settings">The shell settings to store.</param>
        void SaveSettings(ShellSettings settings);
    }

    public class ShellSettingsManager : IShellSettingsManager {
        private readonly IAppDataFolder _appDataFolder;
        private const string _settingsFileNameFormat = "Settings.{0}";

        public ShellSettingsManager(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            var tenantPaths = _appDataFolder
                .ListDirectories("Sites")
                .Select(path => _appDataFolder.MapPath(path));

            var shellSettings = new List<ShellSettings>();

            foreach (var tenantPath in tenantPaths) {
                Logger.Information("ShellSettings found in '{0}', attempting to load.", tenantPath);

                var configurationContainer =
                    new ConfigurationBuilder()
                        .AddJsonFile(_appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "json")),
                            true)
                        .AddXmlFile(_appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "xml")),
                            true)
                        .Add(
                            new DefaultFileConfigurationSource(
                                _appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "txt")), false));

                var config = configurationContainer.Build();

                var shellSetting = new ShellSettings {
                    Name = config.Get("Name"),
                    DataConnectionString = config.Get("DataConnectionString"),
                    DataProvider = config.Get("DataProvider"),
                    DataTablePrefix = config.Get("DataTablePrefix"),
                    RequestUrlHost = config.Get("RequestUrlHost"),
                    RequestUrlPrefix = config.Get("RequestUrlPrefix")
                };

                TenantState state;
                shellSetting.State = Enum.TryParse(config.Get("State"), true, out state)
                    ? state
                    : TenantState.Uninitialized;

                shellSettings.Add(shellSetting);

                Logger.Information("Loaded ShellSettings for tenant '{0}'", shellSetting.Name);
            }

            return shellSettings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSettings) {
            if (shellSettings == null)
                throw new ArgumentNullException(nameof(shellSettings));
            if (string.IsNullOrWhiteSpace(shellSettings.Name))
                throw new ArgumentException(
                    "The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.",
                    nameof(shellSettings.Name));

            Logger.Information("Saving ShellSettings for tenant '{0}'", shellSettings.Name);

            var tenantPath = _appDataFolder.MapPath(_appDataFolder.Combine("Sites", shellSettings.Name));

            var configurationSource = new DefaultFileConfigurationSource(
                _appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "txt")), false);

            foreach (var key in shellSettings.Keys) {
                configurationSource.Set(key, (shellSettings[key] ?? string.Empty).ToString());
            }

            configurationSource.Commit();

            Logger.Information("Saved ShellSettings for tenant '{0}'", shellSettings.Name);
        }
    }
}