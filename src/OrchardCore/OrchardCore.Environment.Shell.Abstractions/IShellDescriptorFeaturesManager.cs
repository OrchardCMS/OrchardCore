using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell
{
    public delegate void FeatureDependencyNotificationHandler(string messageFormat, IFeatureInfo feature, IEnumerable<IFeatureInfo> features);

    public interface IShellDescriptorFeaturesManager
    {
        Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features);
        Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features, bool force);
        Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features);
        Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features, bool force);
    }
}
