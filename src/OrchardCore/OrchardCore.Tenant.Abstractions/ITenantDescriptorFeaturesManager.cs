using OrchardCore.Extensions.Features;
using OrchardCore.Tenant.Descriptor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Tenant
{
    public delegate void FeatureDependencyNotificationHandler(string messageFormat, IFeatureInfo feature, IEnumerable<IFeatureInfo> features);

    public interface ITenantDescriptorFeaturesManager
    {
        Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(
            TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features);
        Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(
            TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features, bool force);
        Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(
            TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features);
        Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(
            TenantDescriptor tenantDescriptor, IEnumerable<IFeatureInfo> features, bool force);
    }
}
