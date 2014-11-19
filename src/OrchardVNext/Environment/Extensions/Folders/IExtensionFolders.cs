using System.Collections.Generic;
using OrchardVNext.Environment.Extensions.Models;

namespace OrchardVNext.Environment.Extensions.Folders {
    public interface IExtensionFolders {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}