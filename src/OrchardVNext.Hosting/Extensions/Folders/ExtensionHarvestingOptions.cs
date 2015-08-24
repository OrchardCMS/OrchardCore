using System.Collections.Generic;

namespace OrchardVNext.Hosting.Extensions.Folders {
    public class ExtensionHarvestingOptions {
        public IList<IModuleLocationExpander> ModuleLocationExpanders { get; }
            = new List<IModuleLocationExpander>();
    }
}
