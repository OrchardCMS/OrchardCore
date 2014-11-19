using System.Collections.Generic;

namespace OrchardVNext.Environment.Configuration {
    public interface IShellSettingsManager {
        IEnumerable<ShellSettings> LoadSettings();
    }

    public class ShellSettingsManager : IShellSettingsManager {
        private const string _settingsFileName = "Settings.txt";

        IEnumerable<ShellSettings> IShellSettingsManager.LoadSettings() {
            //var filePaths = Directory
            //    .GetDirectories(@"D:\Projects\OrchardVNext.Web\src\OrchardVNext.Web\wwwroot\Sites")
            //    .SelectMany(path => Directory.GetFiles(path))
            //    .Where(path => String.Equals(Path.GetFileName(path), _settingsFileName, StringComparison.OrdinalIgnoreCase));

            //foreach (var filePath in filePaths) {
            //    yield return ShellSettingsSerializer.ParseSettings(_appDataFolder.ReadFile(filePath));
            //}

            List<ShellSettings> testShellSettings = new List<ShellSettings>();
            testShellSettings.Add(new ShellSettings { Name = "test1", RequestUrlPrefix = "local.orchardvnext.test1.com", State = TenantState.Running });
            testShellSettings.Add(new ShellSettings { Name = "test2", RequestUrlPrefix = "local.orchardvnext.test2.com", State = TenantState.Running });
            testShellSettings.Add(new ShellSettings { Name = "default", RequestUrlPrefix = "localhost:61562", State = TenantState.Running });

            return testShellSettings;
        }
    }
}