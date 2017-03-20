using System.Threading.Tasks;
using OrchardCore.Tenant.State;

namespace OrchardCore.Tenant
{
    public interface ITenantStateManager
    {
        Task<TenantState> GetTenantStateAsync();
        Task UpdateEnabledStateAsync(TenantFeatureState featureState, TenantFeatureState.State value);
        Task UpdateInstalledStateAsync(TenantFeatureState featureState, TenantFeatureState.State value);
    }
}
