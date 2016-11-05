using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;

namespace Orchard.Environment.Shell
{
    public delegate void FeatureDependencyNotificationHandler(string messageFormat, IFeatureInfo feature, IEnumerable<IFeatureInfo> features);

    public interface IShellDescriptorFeaturesManager
    {
        IEnumerable<IFeatureInfo> EnableFeatures(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features);
        IEnumerable<IFeatureInfo> EnableFeatures(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features, bool force);
        IEnumerable<IFeatureInfo> DisableFeatures(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features);
        IEnumerable<IFeatureInfo> DisableFeatures(
            ShellDescriptor shellDescriptor, IEnumerable<IFeatureInfo> features, bool force);
        IEnumerable<string> GetDependentFeatures(ShellDescriptor shellDescriptor, string featureId);
    }
}
