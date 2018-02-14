using System;
using System.Collections.Generic;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    public class ExtensionInfo : IExtensionInfo
    {
        private readonly string _subPath;
        private readonly IManifestInfo _manifestInfo;
        private readonly IEnumerable<IFeatureInfo> _features;

        public ExtensionInfo(
            string subPath,
            IManifestInfo manifestInfo,
            Func<IManifestInfo, IExtensionInfo, IEnumerable<IFeatureInfo>> features) {

            _subPath = subPath;
            _manifestInfo = manifestInfo;
            _features = features(manifestInfo, this);
        }

        public string Id => _manifestInfo.ModuleInfo.Id;
        public string SubPath => _subPath;
        public IManifestInfo Manifest => _manifestInfo;
        public IEnumerable<IFeatureInfo> Features => _features;
        public bool Exists => _manifestInfo.Exists;
    }
}
