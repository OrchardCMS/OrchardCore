using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;

namespace OrchardCore.Environment.Extensions
{
    public class InternalExtensionInfo : IExtensionInfo
    {
        public InternalExtensionInfo(string subPath)
        {
            Id = Path.GetFileName(subPath);
            SubPath = subPath;

            Manifest = new NotFoundManifestInfo(subPath);
            Features = Enumerable.Empty<IFeatureInfo>();
        }

        public string Id { get; }
        public string SubPath { get; }
        public IManifestInfo Manifest { get; }
        public IEnumerable<IFeatureInfo> Features { get; }
        public bool Exists => Manifest.Exists;
    }
}
