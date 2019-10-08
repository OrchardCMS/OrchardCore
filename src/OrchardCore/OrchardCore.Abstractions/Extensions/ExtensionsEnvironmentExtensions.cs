using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.Environment.Extensions
{
    public static class ExtensionsEnvironmentExtensions
    {
        public static IFileInfo GetExtensionFileInfo(
            this IHostEnvironment environment,
            IExtensionInfo extensionInfo,
            string subPath)
        {
            return environment.ContentRootFileProvider.GetFileInfo(
                Path.Combine(extensionInfo.SubPath, subPath));
        }
    }
}
