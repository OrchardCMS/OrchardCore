using Microsoft.Extensions.Logging;
using OrchardCore.Tenant.State;
using System.Threading.Tasks;

namespace OrchardCore.Tenant
{
    public class NullTenantStateManager : ITenantStateManager
    {
        public NullTenantStateManager(ILogger<NullTenantStateManager> logger)
        {
            Logger = logger;
        }

        ILogger Logger { get; set; }

        public Task<TenantState> GetTenantStateAsync()
        {
            return Task.FromResult(new TenantState());
        }

        public Task UpdateEnabledStateAsync(TenantFeatureState featureState, TenantFeatureState.State value)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Feature {0} EnableState changed from {1} to {2}",
                             featureState.Id, featureState.EnableState, value);
            }

            return Task.CompletedTask;
        }

        public Task UpdateInstalledStateAsync(TenantFeatureState featureState, TenantFeatureState.State value)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Feature {0} InstallState changed from {1} to {2}", featureState.Id, featureState.InstallState, value);
            }

            return Task.CompletedTask;
        }
    }
}
