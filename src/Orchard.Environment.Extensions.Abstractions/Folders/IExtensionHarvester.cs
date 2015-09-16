using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders {
    public interface IExtensionHarvester {
        IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional);
    }
}