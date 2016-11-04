using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Manifests;

namespace Orchard.Environment.Extensions
{
    public class NotFoundExtensionInfo : IExtensionInfo
    {
        private readonly string _extensionId;
        private readonly IFileInfo _fileInfo;

        public NotFoundExtensionInfo(string extensionId) {
            _extensionId = extensionId;
            _fileInfo = new NotFoundFileInfo(_extensionId);
        }

        public IFileInfo ExtensionFileInfo { get { return _fileInfo; } }
        public IFeatureInfoList Features { get; } = new EmptyFeatureInfoList();
        public string Id { get { return _extensionId; } }
        public IManifestInfo Manifest { get { return new NotFoundManifestInfo(_extensionId); } }
        public string SubPath { get; } = string.Empty;
    }
}