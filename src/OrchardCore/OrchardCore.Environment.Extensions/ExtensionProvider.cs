using System.Collections.Generic;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public class ExtensionProvider : IExtensionProvider
    {
        private readonly IFeaturesProvider _featuresProvider;

        /// <summary>
        /// Initializes a new instance of a ExtensionProvider at the given root directory.
        /// </summary>
        /// <param name="featureManager">The feature manager.</param>
        public ExtensionProvider(
            IEnumerable<IFeaturesProvider> featureProviders)
        {
            _featuresProvider = new CompositeFeaturesProvider(featureProviders);
        }

        public int Order { get { return 100; } }

        /// <summary>
        /// Locate an extension at the given path by directly mapping path segments to physical directories.
        /// </summary>
        /// <param name="subpath">A path under the root directory</param>
        /// <returns>The extension information. null returned if extension does not exist</returns>
        public IExtensionInfo GetExtensionInfo(IManifestInfo manifestInfo, string subPath)
        {
            return new ExtensionInfo(subPath, manifestInfo, (mi, ei) => {
                return _featuresProvider.GetFeatures(ei, mi);
            });
        }
    }
}
