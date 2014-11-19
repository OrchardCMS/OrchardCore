using OrchardVNext.Environment.Descriptor.Models;
using OrchardVNext.Environment.Extensions.Models;
using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.Environment.Extensions {
    public interface IExtensionManager {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
        IEnumerable<FeatureDescriptor> AvailableFeatures();

        ExtensionDescriptor GetExtension(string id);

        IEnumerable<Feature> LoadFeatures(IEnumerable<FeatureDescriptor> featureDescriptors);
    }

    public static class ExtensionManagerExtensions {
        public static IEnumerable<FeatureDescriptor> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return extensionManager.AvailableFeatures().Where(fd => descriptor.Features.Any(sf => sf.Name == fd.Id));
        }
    }
}