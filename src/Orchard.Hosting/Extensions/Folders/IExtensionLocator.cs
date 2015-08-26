using System.Collections.Generic;
using Orchard.Hosting.Extensions.Models;

namespace Orchard.Hosting.Extensions.Folders {
    public interface IExtensionLocator {
        IEnumerable<ExtensionDescriptor> AvailableExtensions();
    }
}