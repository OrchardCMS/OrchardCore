using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration
{
    public static class ConfigurationSectionExtensions
    {
        /// <summary>
        /// A helper method that gets a configuration section using the new format (using single underscore) while being backward compatible with the dot-notation.
        /// Not to be used by new code - new code should always use a single underscore when naming configuration keys that require separating segments.
        /// Examples:
        /// Good: "OrchardCore_Media_Azure".
        /// Bad: "OrchardCore.Media.Azure".
        /// See https://github.com/OrchardCMS/OrchardCore/issues/3766.
        /// </summary>
        public static IConfigurationSection GetSectionCompat(this IConfiguration configuration, string key)
        {
            var section = configuration.GetSection(key);

            return section.Exists()
                ? section
                : key.Contains('_')
                    ? configuration.GetSection(key.Replace('_', '.'))
                    : section;
        }

        public static JsonNode AsJsonNode(this IConfiguration configuration)
        {
            if (configuration is IConfigurationSection configurationSection && configurationSection.Value != null)
            {
                return JsonValue.Create(configurationSection.Value);
            }

            var children = configuration.GetChildren().ToList();

            if (children.Count == 0)
            {
                return JsonValue.Create<string>(null);
            }

            if (children[0].Key == "0")
            {
                var array = new JsonArray();

                foreach (var child in children)
                {
                    array.Add(child.AsJsonNode());
                }

                return array;
            }

            var result = new JsonObject();
            foreach (var child in children)
            {
                result.TryAdd(child.Key, child.AsJsonNode());
            }

            return result;
        }
    }
}
