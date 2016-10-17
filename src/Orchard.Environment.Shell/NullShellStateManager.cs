using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell.State;

namespace Orchard.Environment.Shell
{
    public class NullShellStateManager : IShellStateManager
    {
        public NullShellStateManager(ILogger<NullShellStateManager> logger)
        {
            Logger = logger;
        }

        ILogger Logger { get; set; }

        public Task<ShellState> GetShellStateAsync()
        {
            return Task.FromResult(new ShellState());
        }

        public void UpdateEnabledState(ShellFeatureState featureState, ShellFeatureState.State value)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Feature {0} EnableState changed from {1} to {2}",
                             featureState.Name, featureState.EnableState, value);
            }
        }

        public void UpdateInstalledState(ShellFeatureState featureState, ShellFeatureState.State value)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Feature {0} InstallState changed from {1} to {2}", featureState.Name, featureState.InstallState, value);
            }
        }
    }
}
