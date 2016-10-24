using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Manifests;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public class NotFoundExtensionInfo : IExtensionInfo
    {
        public IFileInfo ExtensionFileInfo { get; } = new NotFoundFileInfo(null);
        public IList<IFeatureInfo> Features { get; } = new List<IFeatureInfo>();
        public string Id { get; } = null;
        public IManifestInfo Manifest { get; } = new NotFoundManifestInfo();
        public string SubPath { get; } = string.Empty;
    }
}
