using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public Task<ShellSettings> CreateDefaultSettingsAsync()
        {
            return Task.FromResult(new ShellSettings()
            {
                Name = "Default",
                State = Models.TenantState.Running
            });
        }

        public IEnumerable<ShellSettings> LoadSettings()
        {
            yield return new ShellSettings()
            {
                Name = "Default",
                State = Models.TenantState.Running
            };
        }

        public Task<ShellSettings> LoadSettingsAsync(string tenant) => CreateDefaultSettingsAsync();

        public Task SaveSettingsAsync(ShellSettings shellSettings) => Task.CompletedTask;
    }
}
