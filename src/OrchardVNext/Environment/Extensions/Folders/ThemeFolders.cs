using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public class ThemeFolders : IExtensionFolders {
        private readonly IExtensionHarvester _extensionHarvester;

        public ThemeFolders(IExtensionHarvester extensionHarvester) {
            SearchPaths = new[] { "~/Themes" };
            _extensionHarvester = extensionHarvester;
        }

        public string[] SearchPaths { get; private set; }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(SearchPaths, DefaultExtensionTypes.Theme, "Theme.txt", false/*isManifestOptional*/);
        }
    }
}