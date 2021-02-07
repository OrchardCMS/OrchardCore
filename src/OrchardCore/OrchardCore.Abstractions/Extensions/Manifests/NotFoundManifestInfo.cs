using System.Collections.Generic;
using System.Linq;
using OrchardCore.Modules.Manifest;

namespace OrchardCore.Environment.Extensions.Manifests
{
    public class NotFoundManifestInfo : IManifestInfo
    {
        public bool Exists => false;
        public string Name => null;
        public string Description => null;
        public string Type => null;
        public string Author => null;
        public string Website => null;
        public string Version => null;
        public IEnumerable<string> Tags => Enumerable.Empty<string>();
        public ModuleAttribute ModuleInfo => null;
    }
}
