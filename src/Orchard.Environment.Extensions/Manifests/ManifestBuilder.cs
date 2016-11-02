using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Orchard.Environment.Extensions.Manifests
{
    public class ManifestBuilder : IManifestBuilder
    {
        private readonly IManifestProvider _manifestProvider;
        private readonly ManifestOptions _manifestOptions;
        public ManifestBuilder(IEnumerable<IManifestProvider> manifestProviders,
            IOptions<ManifestOptions> optionsAccessor)
        {
            _manifestProvider = new CompositeManifestProvider(manifestProviders);
            _manifestOptions = optionsAccessor.Value;
        }

        public IManifestInfo GetManifest(string subPath)
        {
            IConfigurationBuilder configurationBuilder 
                = new ConfigurationBuilder();

            foreach (var manifestConfiguration in _manifestOptions.ManifestConfigurations)
            {
                configurationBuilder =
                    _manifestProvider.GetManifestConfiguration(
                        configurationBuilder, 
                        Path.Combine(subPath, manifestConfiguration.ManifestFileName));
            }

            if (!configurationBuilder.Sources.Any())
            {
                return new NotFoundManifestInfo();
            }

            var config = configurationBuilder.Build();

            return new ManifestInfo(config);
        }
    }
}
