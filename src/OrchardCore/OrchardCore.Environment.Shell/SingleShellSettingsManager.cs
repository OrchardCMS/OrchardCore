using System.Collections.Generic;

namespace OrchardCore.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public IEnumerable<ShellSettings> LoadSettings()
        {
            yield return new ShellSettings
            {
                Name = "Default",
                Status = Models.TenantStatus.Running
            };
        }

        public void SaveSettings(ShellSettings shellSettings)
        {
        }
    }
}