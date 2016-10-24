using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Orchard.Environment.Extensions.Manifests
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

            if (!configurationContainer.Sources.Any())
            {
                return new NotFoundManifestInfo();
            }

            var config = configurationContainer.Build();

            return new ManifestInfo(config);
        }
    }
}
