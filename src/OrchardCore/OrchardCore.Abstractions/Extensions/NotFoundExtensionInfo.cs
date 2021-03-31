using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;

namespace OrchardCore.Environment.Extensions
{
    public class NotFoundExtensionInfo : IExtensionInfo
    {
        public NotFoundExtensionInfo(string extensionId)
        {
            Id = extensionId;
            Manifest = new NotFoundManifestInfo();
            Features = Enumerable.Empty<IFeatureInfo>();
        }

        public string Id { get; }
        public string SubPath => Id;
        public IManifestInfo Manifest { get; }
        public IEnumerable<IFeatureInfo> Features { get; }
        public bool Exists => false;
    }
}
