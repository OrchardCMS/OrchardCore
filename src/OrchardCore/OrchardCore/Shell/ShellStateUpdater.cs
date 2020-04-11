using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.State;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Shell
{
    public interface IShellStateUpdater
    {
        Task ApplyChanges();
    }

    public class ShellStateUpdater : IShellStateUpdater
    {
        private readonly ShellSettings _settings;
        private readonly IShellStateManager _stateManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IEnumerable<IFeatureEventHandler> _featureEventHandlers;
        private readonly ILogger _logger;

        public ShellStateUpdater(
            ShellSettings settings,
            IShellStateManager stateManager,
            IExtensionManager extensionManager,
            IEnumerable<IFeatureEventHandler> featureEventHandlers,
            ILogger<ShellStateUpdater> logger)
        {
            _settings = settings;
            _stateManager = stateManager;
            _extensionManager = extensionManager;
            _featureEventHandlers = featureEventHandlers;
            _logger = logger;
        }

        public async Task ApplyChanges()
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Applying changes for for tenant '{TenantName}'", _settings.Name);
            }

            var loadedFeatures = await _extensionManager.LoadFeaturesAsync();

            var shellState = await _stateManager.GetShellStateAsync();

            // merge feature state into ordered list
            var loadedEntries = loadedFeatures
                .Select(fe => new
                {
                    Feature = fe,
                    FeatureDescriptor = fe.FeatureInfo,
                    FeatureState = shellState.Features.FirstOrDefault(s => s.Id == fe.FeatureInfo.Id),
                })
                .Where(entry => entry.FeatureState != null)
                .ToArray();

            // find feature state that is beyond what's currently available from modules
            var additionalState = shellState.Features.Except(loadedEntries.Select(entry => entry.FeatureState));

            // create additional stub entries for the sake of firing state change events on missing features
            var allEntries = loadedEntries.Concat(additionalState.Select(featureState =>
            {
                var featureDescriptor = new InternalFeatureInfo(
                    featureState.Id,
                    new InternalExtensionInfo(featureState.Id)
                    );

                return new
                {
                    Feature = (FeatureEntry)new NonCompiledFeatureEntry(featureDescriptor),
                    FeatureDescriptor = (IFeatureInfo)featureDescriptor,
                    FeatureState = featureState
                };
            })).ToArray();

            // lower enabled states in reverse order
            foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.EnableState == ShellFeatureState.State.Falling))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Disabling feature '{FeatureName}'", entry.Feature.FeatureInfo.Id);
                }

                await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisablingAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
                await _stateManager.UpdateEnabledStateAsync(entry.FeatureState, ShellFeatureState.State.Down);
                await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.DisabledAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
            }

            // lower installed states in reverse order
            foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.InstallState == ShellFeatureState.State.Falling))
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Uninstalling feature '{FeatureName}'", entry.Feature.FeatureInfo.Id);
                }

                await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.UninstallingAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
                await _stateManager.UpdateInstalledStateAsync(entry.FeatureState, ShellFeatureState.State.Down);
                await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.UninstalledAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
            }

            // raise install and enabled states in order
            foreach (var entry in allEntries.Where(entry => IsRising(entry.FeatureState)))
            {
                if (entry.FeatureState.InstallState == ShellFeatureState.State.Rising)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Installing feature '{FeatureName}'", entry.Feature.FeatureInfo.Id);
                    }

                    await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.InstallingAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
                    await _stateManager.UpdateInstalledStateAsync(entry.FeatureState, ShellFeatureState.State.Up);
                    await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.InstalledAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
                }
                if (entry.FeatureState.EnableState == ShellFeatureState.State.Rising)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Enabling feature '{FeatureName}'", entry.Feature.FeatureInfo.Id);
                    }

                    await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.EnablingAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
                    await _stateManager.UpdateEnabledStateAsync(entry.FeatureState, ShellFeatureState.State.Up);
                    await _featureEventHandlers.InvokeAsync((handler, featureInfo) => handler.EnabledAsync(featureInfo), entry.Feature.FeatureInfo, _logger);
                }
            }
        }

        private static bool IsRising(ShellFeatureState state)
        {
            return state.InstallState == ShellFeatureState.State.Rising ||
                   state.EnableState == ShellFeatureState.State.Rising;
        }
    }
}
