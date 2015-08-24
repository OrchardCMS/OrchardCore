using System.Collections.Generic;
using OrchardVNext.Hosting.Extensions.Models;

namespace OrchardVNext.Hosting.Extensions.Folders {
    public interface IExtensionLocator {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}