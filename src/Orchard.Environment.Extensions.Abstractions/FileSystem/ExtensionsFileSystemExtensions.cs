using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Models;
using System.IO;

namespace Orchard.Environment.Extensions.FileSystem
{
    public static class ExtensionsFileSystemExtensions
    {
        public static IFileInfo GetExtensionFileInfo(
            this IHostingEnvironment parentFileSystem,
            ExtensionDescriptor extensionDescriptor)
        {
            return parentFileSystem.ContentRootFileProvider.GetFileInfo(
                Path.Combine(extensionDescriptor.Location, extensionDescriptor.Id));
        }

        public static IFileInfo GetExtensionFileInfo(
            this IHostingEnvironment parentFileSystem,
            ExtensionDescriptor extensionDescriptor,
            string subPath)
        {
            return parentFileSystem.ContentRootFileProvider.GetFileInfo(
                Path.Combine(extensionDescriptor.Location, extensionDescriptor.Id, subPath));
        }
    }
}
