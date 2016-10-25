using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public class ExtensionInfo : IExtensionInfo
    {
        private IFileInfo _fileInfo;
        private string _subPath;
        private IManifestInfo _manifestInfo;
        private IList<IFeatureInfo> _features;

        public ExtensionInfo(
            IFileInfo fileInfo,
            string subPath,
            IManifestInfo manifestInfo,
            Func<IExtensionInfo, IList<IFeatureInfo>> features) {

            _fileInfo = fileInfo;
            _subPath = subPath;
            _manifestInfo = manifestInfo;
            _features = features(this);
        }

        public string Id => _fileInfo.Name;
        public IFileInfo ExtensionFileInfo => _fileInfo;
        public string SubPath => _subPath;
        public IManifestInfo Manifest => _manifestInfo;
        public IList<IFeatureInfo> Features => _features;
    }
}
