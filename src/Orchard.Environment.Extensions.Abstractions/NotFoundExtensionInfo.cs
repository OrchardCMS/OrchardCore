using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Manifests;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public class NotFoundExtensionInfo : IExtensionInfo
    {
        private string _extensionId;
        private IManifestInfo _manifest;

        public NotFoundExtensionInfo(string extensionId, IManifestInfo manifest) {
            _extensionId = extensionId;
            _manifest = manifest;
        }

        public IFileInfo ExtensionFileInfo { get; } = new NotFoundFileInfo(null);
        public IList<IFeatureInfo> Features { get; } = new List<IFeatureInfo>();
        public string Id { get { return _extensionId; } }
        public IManifestInfo Manifest { get { return _manifest; } }
        public string SubPath { get; } = string.Empty;
    }
}
