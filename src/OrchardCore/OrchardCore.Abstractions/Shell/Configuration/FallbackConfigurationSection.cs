using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Environment.Shell.Configuration;

/// <summary>
/// A configuration section that merges a section with a fallback section key by key, a value defined in the
/// section winning over the same value defined in the fallback section. An array defined in the section
/// replaces the array defined in the fallback section, instead of being merged with it.
/// </summary>
/// <remarks>
/// Both sections are expected to be resolved from the same configuration, so that they share the same reload token.
/// </remarks>
internal sealed class FallbackConfigurationSection : IConfigurationSection
{
    private readonly IConfigurationSection _section;
    private readonly IConfigurationSection _fallback;

    public FallbackConfigurationSection(IConfigurationSection section, IConfigurationSection fallback)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(fallback);

        _section = section;
        _fallback = fallback;
    }

    public string Key => _section.Key;

    public string Path => _section.Path;

    public string Value
    {
        get => _section.Value ?? _fallback.Value;
        set => _section.Value = value;
    }

    public string this[string key]
    {
        get => _section[key] ?? _fallback[key];
        set => _section[key] = value;
    }

    public IConfigurationSection GetSection(string key)
        => new FallbackConfigurationSection(_section.GetSection(key), _fallback.GetSection(key));

    public IEnumerable<IConfigurationSection> GetChildren()
    {
        var children = _section.GetChildren().ToArray();

        // The section defines an array, which replaces the array of the fallback section.
        if (children.Length > 0 && children.All(child => int.TryParse(child.Key, NumberStyles.None, CultureInfo.InvariantCulture, out _)))
        {
            return children;
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var child in children)
        {
            keys.Add(child.Key);
        }

        foreach (var child in _fallback.GetChildren())
        {
            keys.Add(child.Key);
        }

        return keys
            .Order(ConfigurationKeyComparer.Instance)
            .Select(GetSection)
            .ToArray();
    }

    public IChangeToken GetReloadToken() => _section.GetReloadToken();
}
