using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Environment.Shell.State;

namespace OrchardCore.Environment.Shell
{
    // This class is registered automatically as transient as it is an event handler
    public class ShellStateCoordinator : IShellDescriptorManagerEventHandler
    {
        private readonly ShellSettings _settings;
        private readonly IShellStateManager _stateManager;

        public ShellStateCoordinator(
            ShellSettings settings,
            IShellStateManager stateManager,
            ILogger<ShellStateCoordinator> logger)
        {
            _settings = settings;
            _stateManager = stateManager;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        async Task IShellDescriptorManagerEventHandler.Changed(ShellDescriptor descriptor, string tenant)
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
                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Adding pending task 'ApplyChanges' for tenant '{TenantName}'", _settings.Name);
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
