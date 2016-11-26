using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Shell
{
    public static class ExtensionManagerExtensions
    {
        public static IEnumerable<IFeatureInfo> GetEnabledFeatures(this IExtensionManager extensionManager, ShellDescriptor shellDescriptor)
        {
            return shellDescriptor
                .Features
                .SelectMany(shellFeature => extensionManager.GetFeatureDependencies(shellFeature.Id)).Distinct();
        }

        public static IEnumerable<IFeatureInfo> GetDisabledFeatures(this IExtensionManager extensionManager, ShellDescriptor shellDescriptor)
        {
            var extensions = extensionManager.GetExtensions();
            var enabledFeatures = extensionManager.GetEnabledFeatures(shellDescriptor);

            return extensions.Features.Except(enabledFeatures);
        }
    }
}