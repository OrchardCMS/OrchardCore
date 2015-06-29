using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.ConfigurationModel;
using OrchardVNext.Environment.Configuration.Sources;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.Environment.Configuration {
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

                IConfigurationSourceRoot configurationContainer =
                    new Microsoft.Framework.ConfigurationModel.Configuration()
                        .AddJsonFile(_appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "json")),
                            true)
                        .AddXmlFile(_appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "xml")),
                            true)
                        .AddIniFile(_appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "ini")),
                            true)
                        .Add(
                            new DefaultFileConfigurationSource(
                                _appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "txt")), false));

                var shellSetting = new ShellSettings {
                    Name = configurationContainer.Get<string>("Name"),
                    DataConnectionString = configurationContainer.Get<string>("DataConnectionString"),
                    DataProvider = configurationContainer.Get<string>("DataProvider"),
                    DataTablePrefix = configurationContainer.Get<string>("DataTablePrefix"),
                    RequestUrlHost = configurationContainer.Get<string>("RequestUrlHost"),
                    RequestUrlPrefix = configurationContainer.Get<string>("RequestUrlPrefix")
                };

                TenantState state;
                shellSetting.State = Enum.TryParse(configurationContainer.Get<string>("State"), true, out state)
                    ? state
                    : TenantState.Uninitialized;

                shellSettings.Add(shellSetting);

                Logger.Information("Loaded ShellSettings for tenant '{0}'", shellSetting.Name);
            }

            return shellSettings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSetting) {
            if (shellSetting == null)
                throw new ArgumentNullException(nameof(shellSetting));
            if (string.IsNullOrWhiteSpace(shellSetting.Name))
                throw new ArgumentException(
                    "The Name property of the supplied ShellSettings object is null or empty; the settings cannot be saved.",
                    nameof(shellSetting.Name));

            Logger.Information("Saving ShellSettings for tenant '{0}'", shellSetting.Name);

            var tenantPath = _appDataFolder.MapPath(_appDataFolder.Combine("Sites", shellSetting.Name));

            var configurationSource = new DefaultFileConfigurationSource(
                _appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "txt")), false);

            configurationSource.Set("Name", shellSetting.Name);
            configurationSource.Set("State", shellSetting.State.ToString());
            configurationSource.Set("DataConnectionString", shellSetting.DataConnectionString);
            configurationSource.Set("DataProvider", shellSetting.DataProvider);
            configurationSource.Set("DataTablePrefix", shellSetting.DataTablePrefix);
            configurationSource.Set("RequestUrlHost", shellSetting.RequestUrlHost);
            configurationSource.Set("RequestUrlPrefix", shellSetting.RequestUrlPrefix);

            configurationSource.Commit();

            Logger.Information("Saved ShellSettings for tenant '{0}'", shellSetting.Name);
        }
    }
}