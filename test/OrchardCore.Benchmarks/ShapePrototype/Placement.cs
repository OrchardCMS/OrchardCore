using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OrchardCore.Benchmarks.ShapePrototype;

/// <summary>
/// A placement rule as compiled from a placement.json entry: plain fields compared directly, instead of a
/// chain of closures built at discovery time.
/// </summary>
public sealed class PlacementRule
{
    public string DisplayType { get; init; }

    public string Differentiator { get; init; }

    public string Location { get; init; }

    public AlternateSet Alternates { get; init; }

    public string[] Wrappers { get; init; }

    public bool Matches(string displayType, string differentiator)
        => (DisplayType is null || string.Equals(DisplayType, displayType, StringComparison.Ordinal))
        && (Differentiator is null || string.Equals(Differentiator, differentiator, StringComparison.Ordinal));
}

/// <summary>
/// The parsed form of a location such as 'Header:5' or 'Content.Metadata:1@group', cached by location string.
/// </summary>
public sealed class ParsedLocation
{
    private static readonly ConcurrentDictionary<string, ParsedLocation> s_cache = new(StringComparer.Ordinal);

    public static readonly ParsedLocation Hidden = new() { IsHidden = true, Zones = [] };

    public string[] Zones { get; private init; }

    public Position Position { get; private init; }

    public bool IsHidden { get; private init; }

    public static ParsedLocation Parse(string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return null;
        }

        return s_cache.GetOrAdd(location, static location =>
        {
            if (location == "-")
            {
                return Hidden;
            }

            var colon = location.IndexOf(':');
            var zones = (colon < 0 ? location : location[..colon]).Split('.');
            var position = colon < 0 ? default : Position.Parse(location[(colon + 1)..]);

            return new ParsedLocation { Zones = zones, Position = position };
        });
    }
}

/// <summary>
/// The placement rules of a theme, by shape type.
/// </summary>
public sealed class PlacementTable
{
    private readonly Dictionary<string, PlacementRule[]> _rules = new(StringComparer.OrdinalIgnoreCase);

    public void Add(string shapeType, PlacementRule rule)
    {
        _rules[shapeType] = _rules.TryGetValue(shapeType, out var existing) ? [.. existing, rule] : [rule];
    }

    public PlacementRule Resolve(string shapeType, string displayType, string differentiator)
    {
        if (!_rules.TryGetValue(shapeType, out var rules))
        {
            return null;
        }

        // The last matching rule wins, as the latest feature overrides the earlier ones.
        for (var i = rules.Length - 1; i >= 0; i--)
        {
            if (rules[i].Matches(displayType, differentiator))
            {
                return rules[i];
            }
        }

        return null;
    }
}

/// <summary>
/// Places the shape of a driver result into the tree: what <c>ShapeResult.ApplyAsync()</c> does today,
/// with the model created directly (no proxy) and the placement rules compiled.
/// </summary>
public static class ShapePlacer
{
    public static ShapeNode Place<TModel>(
        ShapeNode parent,
        PlacementTable placement,
        string shapeType,
        string displayType,
        string differentiator,
        string defaultLocation,
        AlternateSet alternates,
        TModel model)
    {
        var rule = placement.Resolve(shapeType, displayType, differentiator);
        var location = ParsedLocation.Parse(rule?.Location ?? defaultLocation);

        if (location is null || location.IsHidden)
        {
            return null;
        }

        var node = new ShapeNode(shapeType, model)
        {
            DisplayType = displayType,
            Name = differentiator,
            Differentiator = differentiator,
            Alternates = alternates,
            PlacementAlternates = rule?.Alternates,
            Wrappers = rule?.Wrappers,
        };

        var target = parent;
        foreach (var zone in location.Zones)
        {
            target = target.GetOrAddZone(zone);
        }

        target.Add(node, location.Position);

        return node;
    }
}
