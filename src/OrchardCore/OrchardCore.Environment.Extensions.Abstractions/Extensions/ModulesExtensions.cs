using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Extensions
{
    public static class ModulesExtensions
    {
        public static IEnumerable<IExtensionInfo> GetOrderedExtensions(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Select(f => f.Extension);
        }
        public static IEnumerable<IExtensionInfo> GetModules(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static IEnumerable<IExtensionInfo> GetThemes(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsTheme())
                .Select(f => f.Extension);
        }
    }
}