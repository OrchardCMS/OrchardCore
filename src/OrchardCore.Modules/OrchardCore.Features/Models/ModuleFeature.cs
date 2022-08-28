using System.Collections.Generic;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Features.Models
{
    /// <summary>
    /// Represents a module's feature.
    /// </summary>
    public class ModuleFeature
    {
        /// <summary>
        /// The feature descriptor.
        /// </summary>
        public IFeatureInfo Descriptor { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature is enabled.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature is always enabled.
        /// </summary>
        public bool IsAlwaysEnabled { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature needs a data update / migration.
        /// </summary>
        public bool NeedsUpdate { get; set; }

        /// <summary>
        /// Boolean value indicating if the module needs a version update.
        /// </summary>
        public bool NeedsVersionUpdate { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature was recently installed.
        /// </summary>
        public bool IsRecentlyInstalled { get; set; }

        /// <summary>
        /// Boolean value indicating if the feature is enabled by dependency only.
        /// </summary>
        public bool EnabledByDependencyOnly { get; set; }

        /// <summary>
        /// List of enabled features that depend on this feature.
        /// </summary>
        public IEnumerable<IFeatureInfo> EnabledDependentFeatures { get; set; }

        /// <summary>
        /// List of features that this feature depends on.
        /// </summary>
        public IEnumerable<IFeatureInfo> FeatureDependencies { get; set; }
    }
}
