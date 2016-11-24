using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Orchard.Environment.Extensions.Manifests
{
    internal class CompositeManifestProvider : IManifestProvider
    {
        private readonly IManifestProvider[] _manifestProviders;

        public CompositeManifestProvider(params IManifestProvider[] manifestProviders)
        {
            _manifestProviders = manifestProviders ?? new IManifestProvider[0];
        }

        public CompositeManifestProvider(IEnumerable<IManifestProvider> manifestProviders)
        {
            if (manifestProviders == null)
            {
                throw new ArgumentNullException(nameof(manifestProviders));
            }
            _manifestProviders = manifestProviders.ToArray();
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
