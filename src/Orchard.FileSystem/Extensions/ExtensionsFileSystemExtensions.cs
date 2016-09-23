using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Orchard.FileSystem;

namespace Orchard.Environment.Extensions.FileSystem
{
    public static class ExtensionsFileSystemExtensions
    {
        public static IOrchardFileSystem GetSubPathFileProvider(
            this IOrchardFileSystem parentFileSystem,
            string subPath,
            ILogger logger)
        {
            var root = parentFileSystem.GetDirectoryInfo(subPath).FullName;

            return new OrchardFileSystem(root, new PhysicalFileProvider(root), logger);
        }
    }
}
