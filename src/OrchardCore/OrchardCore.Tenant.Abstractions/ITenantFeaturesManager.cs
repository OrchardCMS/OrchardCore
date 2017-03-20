using OrchardCore.Extensions.Features;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Tenant
{
    public interface ITenantFeaturesManager
    {
        Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync();
        Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features);
        Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force);
        Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync();
        Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features);
        Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(IEnumerable<IFeatureInfo> features, bool force);
    }
}
