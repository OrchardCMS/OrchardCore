using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Info
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
        IFileInfo ExtensionFileInfo { get; }

        /// <summary>
        /// The manifest file of the extension
        /// </summary>
        IManifestInfo Manifest { get; }

        /// <summary>
        /// List of features in extension
        /// </summary>
        IList<IFeatureInfo> Features { get; }
    }
}
