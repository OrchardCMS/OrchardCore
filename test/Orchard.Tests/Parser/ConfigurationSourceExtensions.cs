using Microsoft.Framework.Configuration;
using System;

namespace Orchard.Tests.Parser {
    public static class ConfigurationSourceExtensions {
        public static string Get(this IConfigurationSource configSource, string key) {
            string value;

            if (!configSource.TryGet(key, out value)) {
                throw new InvalidOperationException("Key not found");
            }

            return value;
        }
    }
}
