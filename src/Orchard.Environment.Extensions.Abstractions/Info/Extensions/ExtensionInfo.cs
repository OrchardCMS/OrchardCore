using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;

namespace Orchard.Environment.Extensions.Info
{
    public class ExtensionInfo : IExtensionInfo
    {
        private IFileInfo _fileInfo;
        private IManifestInfo _manifestInfo;
        private IList<IFeatureInfo> _features;

        public ExtensionInfo(
            IFileInfo fileInfo,
            IManifestInfo manifestInfo,
            IList<IFeatureInfo> features) {

            _fileInfo = fileInfo;
            _manifestInfo = manifestInfo;
            _features = features;
        }

        public string Id => _fileInfo.Name;
        public IFileInfo Extension => _fileInfo;
        public IManifestInfo Manifest => _manifestInfo;
        public IList<IFeatureInfo> Features => _features;
    }
}
