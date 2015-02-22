using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public class CoreModuleFolders : IExtensionFolders {
        private readonly IExtensionHarvester _extensionHarvester;

        public CoreModuleFolders(IExtensionHarvester extensionHarvester) {
            SearchPaths = new[] { "~/Core/OrchardVNext.Core" };
            _extensionHarvester = extensionHarvester;
        }

        public string[] SearchPaths { get; private set; }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(SearchPaths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}