using System.Collections.Generic;

namespace Orchard.Hosting.Extensions.Folders {
    public class ExtensionHarvestingOptions {
        public IList<IModuleLocationExpander> ModuleLocationExpanders { get; }
            = new List<IModuleLocationExpander>();
    }
}
