using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell
{
    public static class ShellFeaturesManagerExtensions
    {
        public static async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features)
        {
            var (featuresToDisable, featuresToEnable) = await shellFeaturesManager.UpdateFeaturesAsync(new IFeatureInfo[0], features, false);
            return featuresToEnable;
        }

        public static async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features, bool force)
        {
            var (featuresToDisable, featuresToEnable) = await shellFeaturesManager.UpdateFeaturesAsync(new IFeatureInfo[0], features, force);
            return featuresToEnable;
        }

        public static async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features)
        {
            var (featuresToDisable, featuresToEnable) = await shellFeaturesManager.UpdateFeaturesAsync(features, new IFeatureInfo[0], false);
            return featuresToDisable;
        }

        public static async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features, bool force)
        {
            var (featuresToDisable, featuresToEnable) = await shellFeaturesManager.UpdateFeaturesAsync(features, new IFeatureInfo[0], force);
            return featuresToDisable;
        }
    }
}
