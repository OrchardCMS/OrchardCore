using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Environment.Shell
{
    public static class ModulesExtensions
    {
        public static IEnumerable<IExtensionInfo> GetExtensions(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetExtensionsAsync().GetAwaiter().GetResult();
        }

        public static IEnumerable<IExtensionInfo> GetModules(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetModulesAsync().GetAwaiter().GetResult();
        }

        public static IEnumerable<IExtensionInfo> GetThemes(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetThemesAsync().GetAwaiter().GetResult();
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetExtensionsAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetModulesAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetThemesAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsTheme())
                .Select(f => f.Extension);
        }
    }
}