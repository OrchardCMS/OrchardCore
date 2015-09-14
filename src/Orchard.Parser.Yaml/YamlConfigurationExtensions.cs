using System;
using System.IO;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.Configuration.Helper;
using Orchard.Parser.Yaml;

namespace Orchard.Parser {
    public static class IniConfigurationExtensions {
        public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder configuration, string path) {
            if (configuration == null) {
                throw new ArgumentNullException(nameof(configuration));
            }

            return AddYamlFile(configuration, path, optional: false);
        }

        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder configuration,
            string path,
            bool optional) {
            if (configuration == null) {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrEmpty(path)) {
                throw new ArgumentException("InvalidFilePath", nameof(path));
            }

            var fullPath = ConfigurationHelper.ResolveConfigurationFilePath(configuration, path);

            if (!optional && !File.Exists(fullPath)) {
                throw new FileNotFoundException("FormatError_FileNotFound(fullPath)", fullPath);
            }

            configuration.Add(new YamlConfigurationSource(fullPath, optional: optional));
            return configuration;
        }
    }
}
