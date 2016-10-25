using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Shell.Descriptor.Models;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Shell
{
    public class ShellFeaturesManager : IShellFeaturesManager
    {
        private readonly IExtensionManager _extensionManager;

        public ShellFeaturesManager(IExtensionManager extensionManager)
        {
            _extensionManager = extensionManager;
        }


        public IEnumerable<IFeatureInfo> EnabledFeatures(ShellDescriptor shell)
        {
            var extensions = _extensionManager.GetExtensions();
            var features = extensions.Features;

            return features.Where(fd => shell.Features.Any(sf => sf.Name == fd.Id));
        }
    }
}
