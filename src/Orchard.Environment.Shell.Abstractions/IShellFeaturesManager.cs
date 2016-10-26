using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;

namespace Orchard.Environment.Shell
{
    public interface IShellFeaturesManager
    {
        IEnumerable<IFeatureInfo> EnabledFeatures();
        IEnumerable<IFeatureInfo> EnabledFeatures(ShellDescriptor shell);
        IEnumerable<IFeatureInfo> EnableFeatures(IEnumerable<IFeatureInfo> features);
        IEnumerable<IFeatureInfo> EnableFeatures(IEnumerable<IFeatureInfo> features, bool force);
        IEnumerable<IFeatureInfo> DisableFeatures(IEnumerable<IFeatureInfo> features);
        IEnumerable<IFeatureInfo> DisableFeatures(IEnumerable<IFeatureInfo> features, bool force);
    }
}
