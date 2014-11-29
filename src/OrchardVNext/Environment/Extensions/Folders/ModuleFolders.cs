using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public class ModuleFolders : IExtensionFolders {
        private readonly IExtensionHarvester _extensionHarvester;

        public ModuleFolders(IExtensionHarvester extensionHarvester)  {
            SearchPaths = new [] { "~/Modules" };
            _extensionHarvester = extensionHarvester;
        }
        public string[] SearchPaths { get; private set; }
        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(SearchPaths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}