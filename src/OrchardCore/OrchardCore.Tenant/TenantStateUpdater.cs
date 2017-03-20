using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using OrchardCore.Extensions;
using OrchardCore.Tenant.State;
using Orchard.Events;
using OrchardCore.Extensions.Features;
using OrchardCore.Extensions.Manifests;
using System.Threading.Tasks;
using OrchardCore.Extensions.Utility;

namespace OrchardCore.Tenant
{
    public interface ITenantStateUpdater
    {
        Task ApplyChanges();
    }

    public class TenantStateUpdater : ITenantStateUpdater
    {
        private readonly TenantSettings _settings;
        private readonly ITenantStateManager _stateManager;
        private readonly IExtensionManager _extensionManager;
        private readonly IEventBus _eventBus;
        private readonly IDeferredTaskEngine _deferredTaskEngine;

        public TenantStateUpdater(
            TenantSettings settings,
            ITenantStateManager stateManager,
            IExtensionManager extensionManager,
            IEventBus eventBus,
            IDeferredTaskEngine deferredTaskEngine,
            ILogger<TenantStateCoordinator> logger)
        {
            _deferredTaskEngine = deferredTaskEngine;
            _settings = settings;
            _stateManager = stateManager;
            _extensionManager = extensionManager;
            _eventBus = eventBus;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task ApplyChanges()
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Applying changes for for tenant '{0}'", _settings.Name);
            }

            var loadedFeatures = await _extensionManager.LoadFeaturesAsync();

            var tenantState = await _stateManager.GetTenantStateAsync();

            // merge feature state into ordered list
            var loadedEntries = loadedFeatures
                .Select(fe => new
                {
                    Feature = fe,
                    FeatureDescriptor = fe.FeatureInfo,
                    FeatureState = tenantState.Features.FirstOrDefault(s => s.Id == fe.FeatureInfo.Id),
                })
                .Where(entry => entry.FeatureState != null)
                .ToArray();

            // find feature state that is beyond what's currently available from modules
            var additionalState = tenantState.Features.Except(loadedEntries.Select(entry => entry.FeatureState));

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
            foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.EnableState == TenantFeatureState.State.Falling))
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation("Disabling feature '{0}'", entry.Feature.FeatureInfo.Id);
                }

                _eventBus.Notify<IFeatureEventHandler>(x => x.Disabling(entry.Feature.FeatureInfo));
                await _stateManager.UpdateEnabledStateAsync(entry.FeatureState, TenantFeatureState.State.Down);
                _eventBus.Notify<IFeatureEventHandler>(x => x.Disabled(entry.Feature.FeatureInfo));
            }

            // lower installed states in reverse order
            foreach (var entry in allEntries.Reverse().Where(entry => entry.FeatureState.InstallState == TenantFeatureState.State.Falling))
            {
                if (Logger.IsEnabled(LogLevel.Information))
                {
                    Logger.LogInformation("Uninstalling feature '{0}'", entry.Feature.FeatureInfo.Id);
                }

                _eventBus.Notify<IFeatureEventHandler>(x => x.Uninstalling(entry.Feature.FeatureInfo));
                await _stateManager.UpdateInstalledStateAsync(entry.FeatureState, TenantFeatureState.State.Down);
                _eventBus.Notify<IFeatureEventHandler>(x => x.Uninstalled(entry.Feature.FeatureInfo));
            }

            // raise install and enabled states in order
            foreach (var entry in allEntries.Where(entry => IsRising(entry.FeatureState)))
            {
                if (entry.FeatureState.InstallState == TenantFeatureState.State.Rising)
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Installing feature '{0}'", entry.Feature.FeatureInfo.Id);
                    }

                    _eventBus.Notify<IFeatureEventHandler>(x => x.Installing(entry.Feature.FeatureInfo));
                    await _stateManager.UpdateInstalledStateAsync(entry.FeatureState, TenantFeatureState.State.Up);
                    _eventBus.Notify<IFeatureEventHandler>(x => x.Installed(entry.Feature.FeatureInfo));
                }
                if (entry.FeatureState.EnableState == TenantFeatureState.State.Rising)
                {
                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Enabling feature '{0}'", entry.Feature.FeatureInfo.Id);
                    }

                    _eventBus.Notify<IFeatureEventHandler>(x => x.Enabling(entry.Feature.FeatureInfo));
                    await _stateManager.UpdateEnabledStateAsync(entry.FeatureState, TenantFeatureState.State.Up);
                    _eventBus.Notify<IFeatureEventHandler>(x => x.Enabled(entry.Feature.FeatureInfo));
                }
            }
        }

        static bool IsRising(TenantFeatureState state)
        {
            return state.InstallState == TenantFeatureState.State.Rising ||
                   state.EnableState == TenantFeatureState.State.Rising;
        }
    }
}
