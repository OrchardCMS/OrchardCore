using System.Collections.Generic;
using System.Threading.Tasks;

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

        public ShellSettings LoadSettings(string tenant) => CreateDefaultSettings();

        public Task SaveSettingsAsync(ShellSettings shellSettings) => Task.CompletedTask;
    }
}
