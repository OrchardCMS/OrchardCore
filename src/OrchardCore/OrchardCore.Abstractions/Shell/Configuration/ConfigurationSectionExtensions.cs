using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

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

        public static JToken ToJToken(this IConfiguration configuration)
        {
            if (configuration is IConfigurationSection configurationSection && configurationSection.Value != null)
            {
                return JValue.CreateString(configurationSection.Value);
            }

            var children = configuration.GetChildren().ToList();

            if (children.Count == 0)
            {
                return JValue.CreateNull();
            }

            if (children[0].Key == "0")
            {
                var array = new JArray();

                foreach (var child in children)
                {
                    array.Add(child.ToJToken());
                }

                return array;
            }

            var result = new JObject();
            foreach (var child in children)
            {
                result.Add(new JProperty(child.Key, child.ToJToken()));
            }

            return result;
        }
    }
}
