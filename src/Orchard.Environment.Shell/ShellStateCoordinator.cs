using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using Orchard.Environment.Shell.Descriptor.Models;
using Orchard.Environment.Shell.State;

namespace Orchard.Environment.Shell
{
    // This class is registered automatically as transient as it is an event handler
    public class ShellStateCoordinator : IShellDescriptorManagerEventHandler
    {
        private readonly ShellSettings _settings;
        private readonly IShellStateManager _stateManager;
        private readonly IDeferredTaskEngine _deferredTaskEngine;

        public ShellStateCoordinator(
            ShellSettings settings,
            IShellStateManager stateManager,
            IDeferredTaskEngine deferredTaskEngine,
            ILogger<ShellStateCoordinator> logger)
        {
            _deferredTaskEngine = deferredTaskEngine;
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
                var featureName = feature.Name;
                var featureState = shellState.Features.SingleOrDefault(f => f.Name == featureName);
                if (featureState == null)
                {
                    featureState = new ShellFeatureState
                    {
                        Name = featureName
                    };
                }
                if (!featureState.IsInstalled)
                {
                    _stateManager.UpdateInstalledState(featureState, ShellFeatureState.State.Rising);
                }
                if (!featureState.IsEnabled)
                {
                    _stateManager.UpdateEnabledState(featureState, ShellFeatureState.State.Rising);
                }
            }
            foreach (var featureState in shellState.Features)
            {
                var featureName = featureState.Name;
                if (descriptor.Features.Any(f => f.Name == featureName))
                {
                    continue;
                }
                if (!featureState.IsDisabled)
                {
                    _stateManager.UpdateEnabledState(featureState, ShellFeatureState.State.Falling);
                }
            }

            FireApplyChangesIfNeeded();
        }

        private void FireApplyChangesIfNeeded()
        {
            _deferredTaskEngine.AddTask(async context =>
            {
                var stateManager = context.ServiceProvider.GetRequiredService<IShellStateManager>();
                var shellStateUpdater = context.ServiceProvider.GetRequiredService<IShellStateUpdater>();
                var shellState = await stateManager.GetShellStateAsync();

                while (shellState.Features.Any(FeatureIsChanging))
                {
                    var descriptor = new ShellDescriptor
                    {
                        Features = shellState.Features
                            .Where(FeatureShouldBeLoadedForStateChangeNotifications)
                            .Select(x => new ShellFeature
                            {
                                Name = x.Name
                            })
                            .ToArray()
                    };

                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Adding pending task 'ApplyChanges' for shell '{0}'", _settings.Name);
                    }

                    shellStateUpdater.ApplyChanges();
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

        private static bool FeatureShouldBeLoadedForStateChangeNotifications(ShellFeatureState shellFeatureState)
        {
            return FeatureIsChanging(shellFeatureState) || shellFeatureState.EnableState == ShellFeatureState.State.Up;
        }
    }
}
