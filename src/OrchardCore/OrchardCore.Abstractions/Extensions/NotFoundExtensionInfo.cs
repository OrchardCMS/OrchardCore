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
            Features = Enumerable.Empty<IFeatureInfo>();
            Id = extensionId;
            Manifest = new NotFoundManifestInfo(extensionId);
        }

        public bool Exists => false;

        public IEnumerable<IFeatureInfo> Features { get; }

        public string Id { get; }

        public IManifestInfo Manifest { get; }

        public string SubPath => Id;
    }
}
