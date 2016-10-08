using Microsoft.Extensions.Configuration;
using Orchard.Environment.Extensions.Info.Manifests;

namespace Orchard.Environment.Extensions.Info
{
    public class ManifestBuilder : IManifestBuilder
    {
        private readonly IManifestProvider _manifestProvider;
        public ManifestBuilder(IManifestProvider manifestProvider)
        {
            _manifestProvider = manifestProvider;
        }

        public IManifestInfo GetManifest(string subPath)
        {
            var configurationContainer =
                _manifestProvider.GetManifestConfiguration(new ConfigurationBuilder(), subPath);

            var config = configurationContainer.Build();

            return new ManifestInfo(config);
        }
    }
}
