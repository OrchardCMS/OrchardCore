using Microsoft.Extensions.Logging;
using OrchardCore.Tenant.State;
using System.Linq;
using System.Threading.Tasks;
using YesSql.Core.Services;

namespace OrchardCore.Tenant
{
    /// <summary>
    /// Stores <see cref="TenantState"/> in the database.
    /// </summary>
    public class TenantStateManager : ITenantStateManager
    {
        private TenantState _tenantState;
        private readonly ISession _session;

        public TenantStateManager(ISession session, ILogger<TenantStateManager> logger)
        {
            _session = session;
            Logger = logger;
        }

        ILogger Logger { get; set; }

        public async Task<TenantState> GetTenantStateAsync()
        {
            if (_tenantState != null)
            {
                return _tenantState;
            }

            _tenantState = await _session.QueryAsync<TenantState>().FirstOrDefault();

            if (_tenantState == null)
            {
                _tenantState = new TenantState();
                UpdateTenantState();
            }

            return _tenantState;
        }

        public async Task UpdateEnabledStateAsync(TenantFeatureState featureState, TenantFeatureState.State value)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Feature {0} EnableState changed from {1} to {2}",
                             featureState.Id, featureState.EnableState, value);
            }

            var previousFeatureState = await GetOrCreateFeatureStateAsync(featureState.Id);
            if (previousFeatureState.EnableState != featureState.EnableState)
            {
                if (Logger.IsEnabled(LogLevel.Warning))
                {
                    Logger.LogWarning("Feature {0} prior EnableState was {1} when {2} was expected",
                               featureState.Id, previousFeatureState.EnableState, featureState.EnableState);
                }
            }

            previousFeatureState.EnableState = value;
            featureState.EnableState = value;

            UpdateTenantState();
        }

        public async Task UpdateInstalledStateAsync(TenantFeatureState featureState, TenantFeatureState.State value)
        {
            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("Feature {0} InstallState changed from {1} to {2}", featureState.Id, featureState.InstallState, value);
            }

            var previousFeatureState = await GetOrCreateFeatureStateAsync(featureState.Id);
            if (previousFeatureState.InstallState != featureState.InstallState)
            {
                if (Logger.IsEnabled(LogLevel.Warning))
                {
                    Logger.LogWarning("Feature {0} prior InstallState was {1} when {2} was expected",
                               featureState.Id, previousFeatureState.InstallState, featureState.InstallState);
                }
            }

            previousFeatureState.InstallState = value;
            featureState.InstallState = value;

            UpdateTenantState();
        }

        private async Task<TenantFeatureState> GetOrCreateFeatureStateAsync(string id)
        {
            var tenantState = await GetTenantStateAsync();
            var featureState = tenantState.Features.FirstOrDefault(x => x.Id == id);

            if (featureState == null)
            {
                featureState = new TenantFeatureState() { Id = id };
                _tenantState.Features.Add(featureState);
            }

            return featureState;
        }

        private void UpdateTenantState()
        {
            _session.Save(_tenantState);
        }
    }
}
