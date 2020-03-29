using System.Collections.Generic;
using System.Linq;
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

        public Task<IEnumerable<ShellSettings>> LoadSettingsAsync()
        {
            return Task.FromResult((new ShellSettings[] { CreateDefaultSettings() }).AsEnumerable());
        }

        public Task<ShellSettings> LoadSettingsAsync(string tenant) => Task.FromResult(CreateDefaultSettings());

        public Task SaveSettingsAsync(ShellSettings shellSettings) => Task.CompletedTask;
    }
}
