using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Orchard.Environment.Extensions.Manifests
{
    internal class CompositeManifestProvider : IManifestProvider
    {
        private readonly IEnumerable<IManifestProvider> _manifestProviders;

        public CompositeManifestProvider(IEnumerable<IManifestProvider> manifestProviders)
        {
            _manifestProviders = manifestProviders;
        }

        public int Priority
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public IConfigurationBuilder GetManifestConfiguration(
            IConfigurationBuilder configurationBuilder, 
            string subPath)
        {
            foreach (var provider in _manifestProviders)
            {
                configurationBuilder = provider.GetManifestConfiguration(
                    configurationBuilder,
                    subPath
                    );
            }

            return configurationBuilder;
        }
    }
}
