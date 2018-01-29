using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public interface IThemeExtensionInfo : IExtensionInfo { }

    public interface IExtensionInfo
    {
        /// <summary>
        /// The id of the extension
        /// </summary>
        string Id { get; }

        IFileInfo ExtensionFileInfo { get; }

        /// <summary>
        /// The path to the extension
        /// </summary>
        string SubPath { get; }

        bool Exists { get; }

        /// <summary>
        /// The manifest file of the extension
        /// </summary>
        IManifestInfo Manifest { get; }

        /// <summary>
        /// List of features in extension
        /// </summary>
        IEnumerable<IFeatureInfo> Features { get; }
    }
}
