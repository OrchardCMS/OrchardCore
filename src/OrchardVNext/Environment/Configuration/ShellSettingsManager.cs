using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Framework.ConfigurationModel;
using OrchardVNext.Environment.Configuration.Sources;

namespace OrchardVNext.Environment.Configuration {
    public interface IShellSettingsManager {
        IEnumerable<ShellSettings> LoadSettings();
    }

    public class ShellSettingsManager : IShellSettingsManager {
        private const string _settingsFileNameFormat = "Settings.{0}";
        private readonly string[] _settingFileNameExtensions = new string[] { "txt", "json" };

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            var fiolders = Directory
                .GetDirectories(@"D:\Brochard\src\OrchardVNext.Web\wwwroot\Sites");

            var filePaths = Directory
                .GetDirectories(@"D:\Brochard\src\OrchardVNext.Web\wwwroot\Sites")
                .SelectMany(path => Directory.GetFiles(path))
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
                            .Add(new DefaultFileConfigurationSource(filePath));
                        break;
                }

                if (configurationContainer != null) {
                    var shellSetting = new ShellSettings {
                        Name = configurationContainer.Get<string>("Name"),
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