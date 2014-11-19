using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public class ThemeFolders : IExtensionFolders {
        private readonly IEnumerable<string> _paths;
        private readonly IExtensionHarvester _extensionHarvester;

        public ThemeFolders(IExtensionHarvester extensionHarvester) {
            _paths = new[] { "~/Themes" };
            _extensionHarvester = extensionHarvester;
        }

        public IEnumerable<ExtensionDescriptor> AvailableExtensions() {
            return _extensionHarvester.HarvestExtensions(_paths, DefaultExtensionTypes.Theme, "Theme.txt", false/*isManifestOptional*/);
        }
    }
}