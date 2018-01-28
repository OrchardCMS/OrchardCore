using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Extensions
{
    public static class ExtensionManagerExtensions
    {
        public static IEnumerable<IExtensionInfo> GetOrderedExtensions(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f == f.Extension.Features.First())
                .Select(f => f.Extension);
        }

        public static IEnumerable<IExtensionInfo> GetOrderedModules(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f == f.Extension.Features.First())
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static IEnumerable<IExtensionInfo> GetOrderedThemes(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f == f.Extension.Features.First())
                .Where(f => f.Extension.Manifest.IsTheme())
                .Select(f => f.Extension);
        }
    }
}