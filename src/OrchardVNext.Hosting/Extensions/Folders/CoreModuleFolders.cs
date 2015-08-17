using System.Collections.Generic;
using OrchardVNext.Hosting.Extensions.Models;

namespace OrchardVNext.Hosting.Extensions.Folders {
    public class CoreModuleFolders : IExtensionFolders {
        private readonly IExtensionHarvester _extensionHarvester;

        public CoreModuleFolders(IExtensionHarvester extensionHarvester) {
            SearchPaths = new[] { "~/Core/OrchardVNext.Core" };
            _extensionHarvester = extensionHarvester;
        }

        public string[] SearchPaths { get; }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(SearchPaths, DefaultExtensionTypes.Module, "Module.txt", false/*isManifestOptional*/);
        }
    }
}