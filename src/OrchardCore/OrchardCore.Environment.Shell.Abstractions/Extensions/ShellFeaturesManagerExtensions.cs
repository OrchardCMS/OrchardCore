using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Environment.Shell
{
    public static class ShellFeaturesManagerExtensions
    {
        public static IEnumerable<IExtensionInfo> GetEnabledExtensions(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Select(f => f.Extension);
        }

        public static IEnumerable<IExtensionInfo> GetEnabledModules(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsModule())
                .Select(f => f.Extension);
        }

        public static IEnumerable<IExtensionInfo> GetEnabledThemes(this IShellFeaturesManager shellFeaturesManager)
        {
            return shellFeaturesManager.GetEnabledFeaturesAsync().GetAwaiter().GetResult()
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Where(f => f.Extension.Manifest.IsTheme())
                .Select(f => f.Extension);
        }

        public static async Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
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