using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    public class SingleShellSettingsManager : IShellSettingsManager
    {
        public ShellSettings CreateDefaultSettings() => new ShellSettings().AsDefaultShell().AsRunning();

        public Task<IEnumerable<ShellSettings>> LoadSettingsAsync() =>
            Task.FromResult((new ShellSettings[] { CreateDefaultSettings() }).AsEnumerable());

        public Task<IEnumerable<string>> LoadSettingsNamesAsync() =>
            Task.FromResult((new string[] { ShellSettings.DefaultShellName }).AsEnumerable());

        public Task<ShellSettings> LoadSettingsAsync(string tenant) => Task.FromResult(CreateDefaultSettings());

        public Task SaveSettingsAsync(ShellSettings shellSettings) => Task.CompletedTask;

        public Task RemoveSettingsAsync(ShellSettings shellSettings) => Task.CompletedTask;
    }
}
