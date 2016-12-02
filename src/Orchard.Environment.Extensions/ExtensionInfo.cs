using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
using System;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions
{
    public class ExtensionInfo : IExtensionInfo
    {
        private readonly IFileInfo _fileInfo;
        private readonly string _subPath;
        private readonly IManifestInfo _manifestInfo;
        private readonly IEnumerable<IFeatureInfo> _features;

        public ExtensionInfo(
            IFileInfo fileInfo,
            string subPath,
            IManifestInfo manifestInfo,
            Func<IExtensionInfo, IEnumerable<IFeatureInfo>> features) {

            _fileInfo = fileInfo;
            _subPath = subPath;
            _manifestInfo = manifestInfo;
            _features = features(this);
        }

        public string Id => _fileInfo.Name;
        public IFileInfo ExtensionFileInfo => _fileInfo;
        public string SubPath => _subPath;
        public IManifestInfo Manifest => _manifestInfo;
        public IEnumerable<IFeatureInfo> Features => _features;
        public bool Exists => _fileInfo.Exists && _manifestInfo.Exists;
    }
}
