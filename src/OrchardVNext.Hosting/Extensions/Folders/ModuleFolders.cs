using System.Collections.Generic;
using OrchardVNext.Hosting.Extensions.Models;

namespace OrchardVNext.Hosting.Extensions.Folders {
    public class ModuleFolders : IExtensionFolders {
        private readonly IExtensionHarvester _extensionHarvester;

        public ModuleFolders(IExtensionHarvester extensionHarvester)  {
            SearchPaths = new [] { "~/Modules" };
            _extensionHarvester = extensionHarvester;
        }

        public string[] SearchPaths { get; }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(SearchPaths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}