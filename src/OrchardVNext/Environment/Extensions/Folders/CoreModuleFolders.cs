using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public class CoreModuleFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly IExtensionHarvester _extensionHarvester;

        public CoreModuleFolders(IExtensionHarvester extensionHarvester) {
            _paths = new[] { "~/Core" };
            _extensionHarvester = extensionHarvester;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(_paths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}