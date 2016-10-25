using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Manifests
{
    public class NotFoundManifestInfo : IManifestInfo
    {
        public IFileInfo Manifest { get; } = new NotFoundFileInfo(null);
        public bool Exists => false;
        public string Name { get; } = null;
        public string Description { get; } = null;
        public string Type { get; }
        public IConfigurationRoot ConfigurationRoot { get; }
    }
}
