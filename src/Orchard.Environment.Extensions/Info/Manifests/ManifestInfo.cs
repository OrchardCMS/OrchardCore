using System.Collections.Generic;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public class ManifestInfo : IManifestInfo
    {
        private IFileInfo _manifest;
        private IConfigurationRoot _configurationRoot;

        public ManifestInfo(
            IFileInfo manifest,
            IConfigurationRoot configurationRoot)
        {
            _manifest = manifest;
            _configurationRoot = configurationRoot;
        }

        public IFileInfo Manifest => _manifest;
        public bool Exists => Manifest.Exists;
        public string Name => _configurationRoot["name"];
        public string Description => _configurationRoot["description"];
        public string Type => _configurationRoot["type"];
        public IConfigurationRoot ConfigurationRoot => _configurationRoot;
    }
}
