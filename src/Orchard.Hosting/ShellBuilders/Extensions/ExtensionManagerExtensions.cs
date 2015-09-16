using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Hosting.Descriptor.Models;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Hosting.ShellBuilders.Extensions {
    public static class ExtensionManagerExtensions {
        public static IEnumerable<FeatureDescriptor> EnabledFeatures(
            this IExtensionManager extensionManager, ShellDescriptor descriptor) {
            return extensionManager.AvailableFeatures().Where(fd => descriptor.Features.Any(sf => sf.Name == fd.Id));
        }
    }
}
