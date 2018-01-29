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

        public static async Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync(this IShellFeaturesManager shellFeaturesManager)
        {
            return (await shellFeaturesManager.GetEnabledFeaturesAsync())
                .Where(f => f.Id == f.Extension.Features.First().Id)
                .Select(f => f.Extension);
        }
    }
}