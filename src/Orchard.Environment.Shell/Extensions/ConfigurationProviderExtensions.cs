using Microsoft.Extensions.Configuration;
using System;

namespace Orchard.Environment.Shell {
    public static class ConfigurationProviderExtensions {
        public static string Get(this IConfigurationProvider configProvider, string key) {
            string value;

            if (!configProvider.TryGet(key, out value)) {
                throw new InvalidOperationException("Key not found");
            }

            return value;
        }
    }
}
