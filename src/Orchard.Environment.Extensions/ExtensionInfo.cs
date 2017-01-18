using System;
using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;

namespace Orchard.Environment.Extensions
{
    public class ExtensionInfo : IExtensionInfo
    {
        private readonly string _id;
        private readonly IFileInfo _fileInfo;
        private readonly string _subPath;
        private readonly IManifestInfo _manifestInfo;
        private readonly IEnumerable<IFeatureInfo> _features;

        public ExtensionInfo(
            string id,
            IFileInfo fileInfo,
            string subPath,
            IManifestInfo manifestInfo,
            Func<IManifestInfo, IExtensionInfo, IEnumerable<IFeatureInfo>> features) {

            _id = id;
            _fileInfo = fileInfo;
            _subPath = subPath;
            _manifestInfo = manifestInfo;
            _features = features(manifestInfo, this);
        }

        public string Id => _id;
        public IFileInfo ExtensionFileInfo => _fileInfo;
        public string SubPath => _subPath;
        public IManifestInfo Manifest => _manifestInfo;
        public IEnumerable<IFeatureInfo> Features => _features;
        public bool Exists => _fileInfo.Exists && _manifestInfo.Exists;
    }
}
