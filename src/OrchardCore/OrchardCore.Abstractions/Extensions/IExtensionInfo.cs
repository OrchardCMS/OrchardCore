using System.Collections.Generic;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public interface IExtensionInfo
    {
        /// <summary>
        /// The id of the extension
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The path to the extension
        /// </summary>
        string SubPath { get; }

        /// <summary>
        /// The manifest info of the extension
        /// </summary>
        IManifestInfo Manifest { get; }

        /// <summary>
        /// List of features in extension
        /// </summary>
        IEnumerable<IFeatureInfo> Features { get; }

        /// <summary>
        /// Whether this extension info exists
        /// </summary>
        bool Exists { get; }
    }
}
