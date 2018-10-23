using System.Collections.Generic;

namespace OrchardCore.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public IEnumerable<ShellSettings> LoadSettings()
        {
            yield return new ShellSettings
            {
                Name = ShellHelper.DefaultShellName,
                State = Models.TenantState.Running
            };
        }

        public bool TryLoadSettings(string name, out ShellSettings settings)
        {
            if (ShellHelper.DefaultShellName != name)
            {
                settings = null;
                return false;
            }

            settings = new ShellSettings
            {
                Name = ShellHelper.DefaultShellName,
                State = Models.TenantState.Running
            };

            return true;
        }

        public void SaveSettings(ShellSettings shellSettings)
        {
        }
    }
}