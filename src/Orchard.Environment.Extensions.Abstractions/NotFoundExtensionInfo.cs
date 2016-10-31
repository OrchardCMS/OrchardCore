using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
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
        public IFeatureInfoList Features { get; } = null;
        public string Id { get { return _extensionId; } }
        public IManifestInfo Manifest { get { return _manifest; } }
        public string SubPath { get; } = string.Empty;
    }

    
}
