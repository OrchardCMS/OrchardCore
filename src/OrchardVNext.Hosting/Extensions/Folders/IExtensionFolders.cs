using System.Collections.Generic;
using OrchardVNext.Hosting.Extensions.Models;

namespace OrchardVNext.Hosting.Extensions.Folders {
    public interface IExtensionFolders {
        string[] SearchPaths { get; }
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}