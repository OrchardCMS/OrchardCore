using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Framework.ConfigurationModel;
using OrchardVNext.Environment.Configuration.Sources;
using OrchardVNext.FileSystems.AppData;

namespace OrchardVNext.Environment.Configuration {
    public interface IShellSettingsManager {
        IEnumerable<ShellSettings> LoadSettings();
    }

    public class ShellSettingsManager : IShellSettingsManager {
        private readonly IAppDataFolder _appDataFolder;
        private const string _settingsFileNameFormat = "Settings.{0}";
        private readonly string[] _settingFileNameExtensions = new string[] { "txt", "json" };

        public ShellSettingsManager(IAppDataFolder appDataFolder) {
            _appDataFolder = appDataFolder;
        }

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            var filePaths = _appDataFolder
                .ListDirectories("Sites")
                .SelectMany(path => _appDataFolder.ListFiles(path))
                .Where(path => {
                    var filePathName = Path.GetFileName(path);

                    return _settingFileNameExtensions.Any(p =>
                        string.Equals(filePathName, string.Format(_settingsFileNameFormat, p), StringComparison.OrdinalIgnoreCase)
                    );
                });

            List<ShellSettings> shellSettings = new List<ShellSettings>();

            foreach (var filePath in filePaths) {
                IConfigurationSourceContainer configurationContainer = null;

                var extension = Path.GetExtension(filePath);

                switch (extension) {
                    case ".json":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .AddJsonFile(filePath);
                        break;
                    case ".xml":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .AddXmlFile(filePath);
                        break;
                    case ".ini":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .AddIniFile(filePath);
                        break;
                    case ".txt":
                        configurationContainer = new Microsoft.Framework.ConfigurationModel.Configuration()
                            .Add(new DefaultFileConfigurationSource(_appDataFolder, filePath));
                        break;
                }

                if (configurationContainer != null) {
                    var shellSetting = new ShellSettings {
                        Name = configurationContainer.Get<string>("Name"),
                        DataConnectionString = configurationContainer.Get<string>("DataConnectionString"),
                        DataProvider = configurationContainer.Get<string>("DataProvider"),
                        DataTablePrefix = configurationContainer.Get<string>("DataTablePrefix"),
                        RequestUrlHost = configurationContainer.Get<string>("RequestUrlHost"),
                        RequestUrlPrefix = configurationContainer.Get<string>("RequestUrlPrefix")
                    };

                    TenantState state;
                    shellSetting.State = Enum.TryParse(configurationContainer.Get<string>("State"), true, out state) ? state : TenantState.Uninitialized;

                    shellSettings.Add(shellSetting);
                }
            }

            return shellSettings;
        }
    }
}