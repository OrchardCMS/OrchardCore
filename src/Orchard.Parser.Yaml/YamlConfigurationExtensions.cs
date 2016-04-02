using System;
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

            return AddYamlFile(configuration, source => source.Path = path);
        }

        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder builder,
            string path,
            bool optional)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("InvalidFilePath", nameof(path));
            }

            return AddYamlFile(builder, source =>
            {
                source.Path = path;
                source.Optional = optional;
            });
        }

        public static IConfigurationBuilder AddYamlFile(
            this IConfigurationBuilder builder,
            Action<YamlConfigurationSource> configureSource)
        {
            var source = new YamlConfigurationSource();
            configureSource(source);
            builder.Add(source);
            return builder;
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