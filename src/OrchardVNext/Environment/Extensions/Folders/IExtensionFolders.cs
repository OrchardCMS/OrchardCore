using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public interface IExtensionFolders {
        string[] SearchPaths { get; }
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}