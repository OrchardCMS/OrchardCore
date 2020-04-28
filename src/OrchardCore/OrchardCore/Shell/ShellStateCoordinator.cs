using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Environment.Shell.State;

namespace OrchardCore.Environment.Shell
{
    public class ShellStateCoordinator : IShellDescriptorManagerEventHandler
    {
        private readonly IShellStateManager _stateManager;
        private readonly ILogger _logger;

        public ShellStateCoordinator(
            IShellStateManager stateManager,
            ILogger<ShellStateCoordinator> logger)
        {
            _stateManager = stateManager;
            _logger = logger;
        }

        async Task IShellDescriptorManagerEventHandler.ChangedAsync(ShellDescriptor descriptor, ShellSettings settings)
        {
            // deduce and apply state changes involved
            var shellState = await _stateManager.GetShellStateAsync();
            foreach (var feature in descriptor.Features)
            {
                var featureId = feature.Id;
                var featureState = shellState.Features.SingleOrDefault(f => f.Id == featureId);
                if (featureState == null)
                {
                    featureState = new ShellFeatureState
                    {
                        Id = featureId
                    };
                }
                if (!featureState.IsInstalled)
                {
                    await _stateManager.UpdateInstalledStateAsync(featureState, ShellFeatureState.State.Rising);
                }
                if (!featureState.IsEnabled)
                {
                    await _stateManager.UpdateEnabledStateAsync(featureState, ShellFeatureState.State.Rising);
                }
            }
            foreach (var featureState in shellState.Features)
            {
                var featureId = featureState.Id;
                if (descriptor.Features.Any(f => f.Id == featureId))
                {
                    continue;
                }
                if (!featureState.IsDisabled)
                {
                    await _stateManager.UpdateEnabledStateAsync(featureState, ShellFeatureState.State.Falling);
                }
            }

            FireApplyChangesIfNeeded();
        }

        private void FireApplyChangesIfNeeded()
        {
            ShellScope.AddDeferredTask(async scope =>
            {
                var stateManager = scope.ServiceProvider.GetRequiredService<IShellStateManager>();
                var shellStateUpdater = scope.ServiceProvider.GetRequiredService<IShellStateUpdater>();
                var shellState = await stateManager.GetShellStateAsync();

                while (shellState.Features.Any(FeatureIsChanging))
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Adding pending task 'ApplyChanges' for tenant '{TenantName}'", scope.ShellContext.Settings.Name);
                    }

                    await shellStateUpdater.ApplyChanges();
                }
            });
        }

        private static bool FeatureIsChanging(ShellFeatureState shellFeatureState)
        {
            if (shellFeatureState.EnableState == ShellFeatureState.State.Rising ||
                shellFeatureState.EnableState == ShellFeatureState.State.Falling)
            {
                return true;
            }
            if (shellFeatureState.InstallState == ShellFeatureState.State.Rising ||
                shellFeatureState.InstallState == ShellFeatureState.State.Falling)
            {
                return true;
            }

            return false;
        }
    }
}
