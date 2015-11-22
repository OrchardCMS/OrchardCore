using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Folders
{
    public class ExtensionHarvestingOptions
    {
        public IList<IModuleLocationExpander> ModuleLocationExpanders { get; }
            = new List<IModuleLocationExpander>();
    }
}