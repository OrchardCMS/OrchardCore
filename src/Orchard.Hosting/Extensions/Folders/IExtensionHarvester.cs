using System.Collections.Generic;
using Orchard.Hosting.Extensions.Models;

namespace Orchard.Hosting.Extensions.Folders {
    public interface IExtensionHarvester {
        IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional);
    }
}