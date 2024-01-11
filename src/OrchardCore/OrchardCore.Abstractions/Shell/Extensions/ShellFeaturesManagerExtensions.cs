using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell
{
    public static class ShellFeaturesManagerExtensions
    {
        public static Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features)
        {
            return shellFeaturesManager.EnableFeaturesAsync(features, false);
        }

        public static async Task<IEnumerable<IFeatureInfo>> EnableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features, bool force)
        {
            var (_, featuresToEnable) = await shellFeaturesManager.UpdateFeaturesAsync(Array.Empty<IFeatureInfo>(), features, force);
            return featuresToEnable;
        }

        public static Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features)
        {
            return shellFeaturesManager.DisableFeaturesAsync(features, false);
        }

        public static async Task<IEnumerable<IFeatureInfo>> DisableFeaturesAsync(this IShellFeaturesManager shellFeaturesManager,
            IEnumerable<IFeatureInfo> features, bool force)
        {
            var (featuresToDisable, _) = await shellFeaturesManager.UpdateFeaturesAsync(features, Array.Empty<IFeatureInfo>(), force);
            return featuresToDisable;
        }
    }
}
