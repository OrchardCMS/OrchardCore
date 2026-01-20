using System.Collections.Generic;

namespace OrchardCore.Environment.Extensions.Features
{
    /// <summary>
    /// Provides opportunities to work with the Module Feature set.
    /// </summary>
    public interface IFeaturesProvider
    {
        /// <summary>
        /// Returns the Features corresponding with the Extension, Manifest, etc.
        /// </summary>
        /// <param name="extensionInfo"></param>
        /// <param name="manifestInfo"></param>
        /// <returns></returns>
        IEnumerable<IFeatureInfo> GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo);
    }
}
