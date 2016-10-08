using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Info.Manifests
{
    public class ManifestInfo : IManifestInfo
    {
        private IConfigurationRoot _configurationRoot;

        public ManifestInfo(
            IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        public bool Exists => true;
        public string Name => _configurationRoot["name"];
        public string Description => _configurationRoot["description"];
        public string Type => _configurationRoot["type"];
        public IConfigurationRoot ConfigurationRoot => _configurationRoot;
    }
}
