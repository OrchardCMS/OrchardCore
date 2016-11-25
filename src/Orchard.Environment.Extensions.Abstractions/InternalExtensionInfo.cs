using System;
using Microsoft.Extensions.FileProviders;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Manifests;

namespace Orchard.Environment.Extensions
{
    public class InternalExtensionInfo : IExtensionInfo
    {
        private readonly IFileInfo _fileInfo;
        private readonly string _subPath;
        private readonly IManifestInfo _manifestInfo;
        private readonly IFeatureInfoList _features;

        public InternalExtensionInfo(string subPath)
        {
            _subPath = subPath;

            _fileInfo = new NotFoundFileInfo(subPath);
            _manifestInfo = new NotFoundManifestInfo(subPath);
            _features = EmptyFeatureInfoList.Singleton;
        }
        
        public string Id => _fileInfo.Name;
        public IFileInfo ExtensionFileInfo => _fileInfo;
        public string SubPath => _subPath;
        public IManifestInfo Manifest => _manifestInfo;
        public IFeatureInfoList Features => _features;
        public bool Exists => _fileInfo.Exists && _manifestInfo.Exists;
    }
}