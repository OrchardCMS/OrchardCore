using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Modules;

namespace OrchardCore.Environment.Extensions
{
    public class NotFoundExtensionInfo : IExtensionInfo
    {
        public NotFoundExtensionInfo(string extensionId)
        {
            Id = extensionId;
            SubPath = Application.ModulesRoot + extensionId;
            Manifest = new NotFoundManifestInfo();
            Features = Enumerable.Empty<IFeatureInfo>();
        }

        public string Id { get; }
        public string SubPath { get; }
        public IManifestInfo Manifest { get; }
        public IEnumerable<IFeatureInfo> Features { get; }
        public bool Exists => false;
    }
}
