using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Environment.Extensions
{
    public static class ExtensionManagerExtensions
    {
        public static IEnumerable<IExtensionInfo> GetOrderedExtensions(this IExtensionManager extensionManager)
        {
            return extensionManager.GetFeatures()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Select(f => f.Extension);
        }
    }
}