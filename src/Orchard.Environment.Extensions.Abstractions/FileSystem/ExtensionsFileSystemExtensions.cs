using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions.Models;
using Orchard.FileSystem;

namespace Orchard.Environment.Extensions.FileSystem
{
    public static class ExtensionsFileSystemExtensions
    {
        public static IOrchardFileSystem GetExtensionFileProvider(
            this IOrchardFileSystem parentFileSystem,
            ExtensionDescriptor extensionDescriptor,
            ILogger logger)
        {
            var subPath = parentFileSystem.Combine(extensionDescriptor.Location, extensionDescriptor.Id);

            return parentFileSystem
                .GetSubPathFileProvider(subPath, logger);
        }
    }
}
