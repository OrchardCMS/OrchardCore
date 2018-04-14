using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public ShellSettings GetSettings(string name)
        {
            return LoadSettings().First();
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            yield return new ShellSettings
            {
                Name = "Default",
                State = Models.TenantState.Running
            };
        }

        public void SaveSettings(ShellSettings shellSettings)
        {
        }

        public bool TryGetSettings(string name, out ShellSettings settings)
        {
            settings = LoadSettings().First();
            return true;
        }
    }
}