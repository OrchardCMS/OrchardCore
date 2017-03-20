using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.DeferredTasks;
using OrchardCore.Tenant.Descriptor.Models;
using OrchardCore.Tenant.State;

namespace OrchardCore.Tenant
{
    // This class is registered automatically as transient as it is an event handler
    public class TenantStateCoordinator : ITenantDescriptorManagerEventHandler
    {
        private readonly TenantSettings _settings;
        private readonly ITenantStateManager _stateManager;
        private readonly IDeferredTaskEngine _deferredTaskEngine;

        public TenantStateCoordinator(
            TenantSettings settings,
            ITenantStateManager stateManager,
            IDeferredTaskEngine deferredTaskEngine,
            ILogger<TenantStateCoordinator> logger)
        {
            _deferredTaskEngine = deferredTaskEngine;
            _settings = settings;
            _stateManager = stateManager;
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        async Task ITenantDescriptorManagerEventHandler.Changed(TenantDescriptor descriptor, string tenant)
        {
            // deduce and apply state changes involved
            var tenantState = await _stateManager.GetTenantStateAsync();
            foreach (var feature in descriptor.Features)
            {
                var featureId = feature.Id;
                var featureState = tenantState.Features.SingleOrDefault(f => f.Id == featureId);
                if (featureState == null)
                {
                    featureState = new TenantFeatureState
                    {
                        Id = featureId
                    };
                }
                if (!featureState.IsInstalled)
                {
                    await _stateManager.UpdateInstalledStateAsync(featureState, TenantFeatureState.State.Rising);
                }
                if (!featureState.IsEnabled)
                {
                    await _stateManager.UpdateEnabledStateAsync(featureState, TenantFeatureState.State.Rising);
                }
            }
            foreach (var featureState in tenantState.Features)
            {
                var featureId = featureState.Id;
                if (descriptor.Features.Any(f => f.Id == featureId))
                {
                    continue;
                }
                if (!featureState.IsDisabled)
                {
                    await _stateManager.UpdateEnabledStateAsync(featureState, TenantFeatureState.State.Falling);
                }
            }

            FireApplyChangesIfNeeded();
        }

        private void FireApplyChangesIfNeeded()
        {
            _deferredTaskEngine.AddTask(async context =>
            {
                var stateManager = context.ServiceProvider.GetRequiredService<ITenantStateManager>();
                var tenantStateUpdater = context.ServiceProvider.GetRequiredService<ITenantStateUpdater>();
                var tenantState = await stateManager.GetTenantStateAsync();

                while (tenantState.Features.Any(FeatureIsChanging))
                {
                    var descriptor = new TenantDescriptor
                    {
                        Features = tenantState.Features
                            .Where(FeatureShouldBeLoadedForStateChangeNotifications)
                            .Select(x => new TenantFeature
                            {
                                Id = x.Id
                            })
                            .ToArray()
                    };

                    if (Logger.IsEnabled(LogLevel.Information))
                    {
                        Logger.LogInformation("Adding pending task 'ApplyChanges' for tenant '{0}'", _settings.Name);
                    }

                    await tenantStateUpdater.ApplyChanges();
                }
            });
        }

        private static bool FeatureIsChanging(TenantFeatureState tenantFeatureState)
        {
            if (tenantFeatureState.EnableState == TenantFeatureState.State.Rising ||
                tenantFeatureState.EnableState == TenantFeatureState.State.Falling)
            {
                return true;
            }
            if (tenantFeatureState.InstallState == TenantFeatureState.State.Rising ||
                tenantFeatureState.InstallState == TenantFeatureState.State.Falling)
            {
                return true;
            }
            return false;
        }

        private static bool FeatureShouldBeLoadedForStateChangeNotifications(TenantFeatureState tenantFeatureState)
        {
            return FeatureIsChanging(tenantFeatureState) || tenantFeatureState.EnableState == TenantFeatureState.State.Up;
        }
    }
}
