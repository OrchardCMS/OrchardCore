using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.State;

namespace OrchardCore.Environment.Shell
{
    public class NullShellStateManager : IShellStateManager
    {
        private readonly ILogger _logger;

        public NullShellStateManager(ILogger<NullShellStateManager> logger)
        {
            _logger = logger;
        }

        public Task<ShellState> GetShellStateAsync()
        {
            return Task.FromResult(new ShellState());
        }

        public Task UpdateEnabledStateAsync(ShellFeatureState featureState, ShellFeatureState.State value)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Feature '{FeatureName}' EnableState changed from '{FeatureState}' to '{FeatureState}'",
                             featureState.Id, featureState.EnableState, value);
            }

            return Task.CompletedTask;
        }

        public Task UpdateInstalledStateAsync(ShellFeatureState featureState, ShellFeatureState.State value)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Feature '{FeatureName}' InstallState changed from '{FeatureState}' to '{FeatureState}'", featureState.Id, featureState.InstallState, value);
            }

            return Task.CompletedTask;
        }
    }
}
