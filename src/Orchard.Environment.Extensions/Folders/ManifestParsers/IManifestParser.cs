using Orchard.Environment.Extensions.Models;

namespace Orchard.Environment.Extensions.Folders.ManifestParsers {
    public interface IManifestParser {
        string Extension { get; }
        ExtensionDescriptor GetDescriptorForExtension(string path, string extensionId, string extensionType, string manifestText);
    }
}