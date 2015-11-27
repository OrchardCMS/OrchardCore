using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Orchard.Parser.Yaml;

namespace Orchard.Parser
{
    public static class YamlConfigurationExtensions
    {
        public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder configuration, string path)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            return AddYamlFile(configuration, path, optional: false);
        }

        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder configurationBuilder,
            string path,
            bool optional)
        {
            if (configurationBuilder == null)
            {
                throw new ArgumentNullException(nameof(configurationBuilder));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("InvalidFilePath", nameof(path));
            }

            var fullPath = Path.Combine(configurationBuilder.GetBasePath(), path);

            if (!optional && !File.Exists(fullPath))
            {
                throw new FileNotFoundException("FormatError_FileNotFound(fullPath)", fullPath);
            }

            configurationBuilder.Add(new YamlConfigurationProvider(fullPath, optional: optional));
            return configurationBuilder;
        }

        public static string Get(this IConfigurationProvider provider, string key)
        {
            string value;

            if (!provider.TryGet(key, out value))
            {
                throw new InvalidOperationException("Key not found");
            }

            return value;
        }
    }
}