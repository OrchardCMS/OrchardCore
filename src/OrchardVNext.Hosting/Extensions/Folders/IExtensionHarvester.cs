using System.Collections.Generic;
using OrchardVNext.Hosting.Extensions.Models;

namespace OrchardVNext.Hosting.Extensions.Folders {
    public interface IExtensionHarvester {
        IEnumerable<ExtensionDescriptor> HarvestExtensions(IEnumerable<string> paths, string extensionType, string manifestName, bool manifestIsOptional);
    }
}