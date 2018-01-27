using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Environment.Shell
{
    public static class ModulesExtensions
    {
        public static IEnumerable<IExtensionInfo> GetEnabledExtensions(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetEnabledExtensionsAsync().GetAwaiter().GetResult();
        }

        public static IEnumerable<IExtensionInfo> GetEnabledModules(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetEnabledModulesAsync().GetAwaiter().GetResult();
        }

        public static IEnumerable<IExtensionInfo> GetEnabledThemes(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetEnabledThemesAsync().GetAwaiter().GetResult();
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetEnabledModulesAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetEnabledThemesAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsTheme())
                .Select(f => f.Extension);
        }
    }
}