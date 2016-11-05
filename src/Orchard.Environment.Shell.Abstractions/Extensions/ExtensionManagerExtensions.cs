using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Shell
{
    public static class ExtensionManagerExtensions
    {
        public static IEnumerable<IFeatureInfo> EnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor shellDescriptor)
        {
            var extensions = extensionManager.GetExtensions();
            var features = extensions.Features;

            return features.Where(fd => shellDescriptor.Features.Any(sf => sf.Name == fd.Id));
        }

        public static IEnumerable<IFeatureInfo> DisabledFeatures(this IExtensionManager extensionManager, ShellDescriptor shellDescriptor)
        {
            var extensions = extensionManager.GetExtensions();
            var features = extensions.Features;

            return features.Where(fd => shellDescriptor.Features.All(sf => sf.Name != fd.Id));
        }
    }
}