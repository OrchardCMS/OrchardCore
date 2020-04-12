using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Extensions.Utility
{
    /// <summary>
    /// A helper class that gets a configuration section using the new format (using single underscore) while being backward compatible with the dot-notation.
    /// Not to be used by new code - new code should always use a single underscore when naming configuration keys that require separating segments.
    /// Examples:
    /// Good: "OrchardCore_Media_Azure".
    /// Bad: "OrchardCore.Media.Azure".
    /// See https://github.com/OrchardCMS/OrchardCore/issues/3766.
    /// </summary>
    public static class ConfigurationSectionExtensions
    {
        public static IConfigurationSection GetSection(this IConfiguration configuration, string key, string compatKey)
        {
            var section = configuration.GetSection(key);
            return section.Exists() ? section : configuration.GetSection(compatKey);
        }
    }
}
