using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;

namespace OrchardCore.Environment.Shell.Configuration;

public static class ConfigurationSectionExtensions
{
    /// <summary>
    /// A helper method that gets a configuration section using the underscore format while being backward compatible with the dot-notation,
    /// e.g. "OrchardCore_Media_Azure" and "OrchardCore.Media.Azure". See https://github.com/OrchardCMS/OrchardCore/issues/3766.
    /// Not to be used by new code - new code should not prefix configuration sections with "OrchardCore_", and should use nesting
    /// to group related sections, e.g. "Media:Azure" for the "OrchardCore:Media:Azure" section.
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

    /// <summary>
    /// Gets a configuration section that is still supported under one or more legacy names, to bind options or read
    /// values from the section and its legacy sections at once.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The values of the sections are merged key by key: a value defined in the section wins over the same value defined
    /// in a legacy section, and a legacy section wins over the next legacy sections. An array defined in a section replaces
    /// the same array defined in the next sections, instead of being merged with it.
    /// </para>
    /// <para>
    /// Legacy keys using underscores are also resolved using the older dot-notation, e.g. <c>OrchardCore.Media.Azure</c>.
    /// </para>
    /// </remarks>
    /// <param name="configuration">The configuration to get the sections from.</param>
    /// <param name="key">The key of the configuration section, e.g. <c>Media:Azure</c>.</param>
    /// <param name="legacyKeys">The deprecated keys of the configuration section ordered by priority, e.g. <c>OrchardCore_Media_Azure</c>.</param>
    public static IConfigurationSection GetSectionCompat(this IConfiguration configuration, string key, params string[] legacyKeys)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(legacyKeys);

        var section = configuration.GetSection(key);

        if (legacyKeys.Length == 0)
        {
            return section;
        }

        // Chain the legacy sections from the lowest to the highest priority.
        IConfigurationSection fallback = null;

        for (var i = legacyKeys.Length - 1; i >= 0; i--)
        {
            ArgumentException.ThrowIfNullOrEmpty(legacyKeys[i], nameof(legacyKeys));

            var legacySection = configuration.GetSectionCompat(legacyKeys[i]);

            fallback = fallback is null
                ? legacySection
                : new FallbackConfigurationSection(legacySection, fallback);
        }

        return new FallbackConfigurationSection(section, fallback);
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
            return null;
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
