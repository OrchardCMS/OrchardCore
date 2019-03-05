using System.Collections.Generic;

namespace OrchardCore.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public ShellSettings CreateDefaultSettings()
        {
            return new ShellSettings()
            {
                Name = "Default",
                State = Models.TenantState.Running
            };
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            yield return new ShellSettings()
            {
                Name = "Default",
                State = Models.TenantState.Running
            };
        }

        public void SaveSettings(ShellSettings shellSettings)
        {
        }
    }
}