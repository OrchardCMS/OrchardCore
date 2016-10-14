using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public class NotFoundManifestInfo : IManifestInfo
    {
        public IFileInfo Manifest { get; private set; }
        public bool Exists => false;
        public string Name { get; } = null;
        public string Description { get; } = null;
        public string Type { get; private set; }
        public IConfigurationRoot ConfigurationRoot { get; }
    }
}
