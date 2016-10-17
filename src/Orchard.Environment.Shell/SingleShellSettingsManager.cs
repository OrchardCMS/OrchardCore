using System.Collections.Generic;

namespace Orchard.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public IEnumerable<ShellSettings> LoadSettings()
        {
            var shellSettings = new List<ShellSettings>();
            shellSettings.Add(new ShellSettings
            {
                Name = "Default",
                State = Models.TenantState.Running
            });

            return shellSettings;
        }

        public void SaveSettings(ShellSettings shellSettings)
        {
        }
    }
}