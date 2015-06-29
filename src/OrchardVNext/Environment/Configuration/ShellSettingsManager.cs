using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.ConfigurationModel;
using OrchardVNext.Environment.Configuration.Sources;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.Environment.Configuration {
    public interface IShellSettingsManager {
        IEnumerable<ShellSettings> LoadSettings();
        void SaveSettings(ShellSettings shellSettings);
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
            }

            return shellSettings;
        }

        void IShellSettingsManager.SaveSettings(ShellSettings shellSettings) {

            var tenantPath = _appDataFolder.MapPath(_appDataFolder.Combine("Sites", shellSettings.Name));

            var configurationSource = new DefaultFileConfigurationSource(
                _appDataFolder.Combine(tenantPath, string.Format(_settingsFileNameFormat, "txt")), false);

            configurationSource.Set("Name", shellSettings.Name);
            configurationSource.Set("State", shellSettings.State.ToString());
            configurationSource.Set("DataConnectionString", shellSettings.DataConnectionString);
            configurationSource.Set("DataProvider", shellSettings.DataProvider);
            configurationSource.Set("DataTablePrefix", shellSettings.DataTablePrefix);
            configurationSource.Set("RequestUrlHost", shellSettings.RequestUrlHost);
            configurationSource.Set("RequestUrlPrefix", shellSettings.RequestUrlPrefix);

            configurationSource.Commit();
        }
    }
}