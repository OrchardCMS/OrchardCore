using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public class ModuleFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly IExtensionHarvester _extensionHarvester;

        public ModuleFolders(IExtensionHarvester extensionHarvester)  {
            _paths = new [] { "~/Modules" };
            _extensionHarvester = extensionHarvester;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(_paths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}