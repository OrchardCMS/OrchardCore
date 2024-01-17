using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Shell
{
    public interface IShellFeaturesManager
    {
        Task<IEnumerable<IFeatureInfo>> GetAvailableFeaturesAsync();
        Task<IEnumerable<IFeatureInfo>> GetEnabledFeaturesAsync();
        Task<IEnumerable<IFeatureInfo>> GetAlwaysEnabledFeaturesAsync();
        Task<IEnumerable<IFeatureInfo>> GetDisabledFeaturesAsync();
        Task<(IEnumerable<IFeatureInfo>, IEnumerable<IFeatureInfo>)> UpdateFeaturesAsync(
            IEnumerable<IFeatureInfo> featuresToDisable, IEnumerable<IFeatureInfo> featuresToEnable, bool force);
        Task<IEnumerable<IExtensionInfo>> GetEnabledExtensionsAsync();
    }
}
