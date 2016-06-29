using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Folders
{
    public class ExtensionHarvestingOptions
    {
        public IList<IExtensionLocationExpander> ExtensionLocationExpanders { get; }
            = new List<IExtensionLocationExpander>();
    }
}