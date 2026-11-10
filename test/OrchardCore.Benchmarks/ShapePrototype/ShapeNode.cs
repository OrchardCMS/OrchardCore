using System;
using System.Collections.Generic;

namespace OrchardCore.Benchmarks.ShapePrototype;

/// <summary>
/// A set of alternates shared by every shape rendered with the same conventions, e.g. all the 'TitlePart'
/// shapes of a 'BlogPost' in the 'Summary' display type. It is cached by the code that computes it, so
/// a shape references it instead of copying its names, and binding resolution is cached per set reference.
/// </summary>
public sealed class AlternateSet
{
    public static readonly AlternateSet Empty = new([]);

    public AlternateSet(string[] names)
    {
        Names = names;
    }

    /// <summary>
    /// The alternates, from the lowest to the highest priority.
    /// </summary>
    public string[] Names { get; }
}

/// <summary>
/// What a binding is resolved from: the shape type and the alternate sets that apply to it. Two shapes with
/// the same key always render with the same binding, so the resolution is cached by key.
/// </summary>
public readonly struct ShapeKey : IEquatable<ShapeKey>
{
    public ShapeKey(string type, AlternateSet alternates, AlternateSet moreAlternates = null)
    {
        Type = type;
        Alternates = alternates ?? AlternateSet.Empty;
        MoreAlternates = moreAlternates ?? AlternateSet.Empty;
    }

    public string Type { get; }

    /// <summary>
    /// The alternates of the shape, usually the set of its conventions, e.g. from the part and content type.
    /// </summary>
    public AlternateSet Alternates { get; }

    /// <summary>
    /// A second set, e.g. added by placement, with a higher priority than <see cref="Alternates"/>.
    /// </summary>
    public AlternateSet MoreAlternates { get; }

    public bool Equals(ShapeKey other)
        => ReferenceEquals(Alternates, other.Alternates)
        && ReferenceEquals(MoreAlternates, other.MoreAlternates)
        && string.Equals(Type, other.Type, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object obj) => obj is ShapeKey other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(Type),
            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(Alternates),
            System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(MoreAlternates));
}

/// <summary>
/// A node of the tree to render. It is not the model: the model is any CLR object, the node only carries
/// what the renderer and the templates need to find a binding and compose the output.
/// </summary>
public sealed class ShapeNode
{
    private List<ShapeNode> _children;
    private bool _sorted;
    private KeyValuePair<string, ShapeNode>[] _zones;
    private int _zoneCount;

    public ShapeNode(string type, object model = null)
    {
        Type = type;
        Model = model;
    }

    public string Type { get; set; }

    public object Model { get; set; }

    public string DisplayType { get; set; }

    public string Name { get; set; }

    public string Differentiator { get; set; }

    public string Prefix { get; set; }

    public AlternateSet Alternates { get; set; }

    public AlternateSet PlacementAlternates { get; set; }

    public string[] Wrappers { get; set; }

    public Position Position { get; set; }

    public ShapeKey Key => new(Type, Alternates, PlacementAlternates);

    public bool HasChildren => _children is { Count: > 0 };

    /// <summary>
    /// The children, sorted by position on first read.
    /// </summary>
    public List<ShapeNode> Children
    {
        get
        {
            _children ??= [];

            if (!_sorted && _children.Count > 1)
            {
                _children.Sort(static (a, b) => a.Position.CompareTo(b.Position));
                _sorted = true;
            }

            return _children;
        }
    }

    public void Add(ShapeNode child, Position position = default)
    {
        child.Position = position;
        (_children ??= []).Add(child);
        _sorted = false;
    }

    /// <summary>
    /// Gets a zone, which is a child node of type 'Zone', or null. Zones are stored in a small array:
    /// a shape has a handful of them and most templates read them in order.
    /// </summary>
    public ShapeNode GetZone(string name)
    {
        for (var i = 0; i < _zoneCount; i++)
        {
            if (string.Equals(_zones[i].Key, name, StringComparison.Ordinal))
            {
                return _zones[i].Value;
            }
        }

        return null;
    }

    public ShapeNode GetOrAddZone(string name)
    {
        var zone = GetZone(name);
        if (zone is not null)
        {
            return zone;
        }

        zone = new ShapeNode("Zone") { Name = name };

        if (_zones is null)
        {
            _zones = new KeyValuePair<string, ShapeNode>[4];
        }
        else if (_zoneCount == _zones.Length)
        {
            Array.Resize(ref _zones, _zones.Length * 2);
        }

        _zones[_zoneCount++] = new(name, zone);
        return zone;
    }
}

/// <summary>
/// A position parsed once, when the shape is placed, so that sorting compares integers and not strings.
/// Supports the 'before' / 'after' anchors and dotted segments ('5.1'). A real implementation would also
/// cover the 'start' / 'end' sentinels and free text; this is enough to measure the approach.
/// </summary>
public readonly struct Position : IComparable<Position>
{
    private readonly long _packed;

    private Position(long packed) => _packed = packed;

    public static Position Parse(string position)
    {
        if (string.IsNullOrEmpty(position))
        {
            return default;
        }

        if (position == "before")
        {
            return new(int.MinValue + 1L << 32);
        }

        if (position == "after")
        {
            return new((int.MaxValue - 1L) << 32);
        }

        var dot = position.IndexOf('.');
        var major = int.Parse(dot < 0 ? position : position.AsSpan(0, dot));
        var minor = dot < 0 ? 0 : int.Parse(position.AsSpan(dot + 1));

        return new(((long)major << 32) | (uint)minor);
    }

    public int CompareTo(Position other) => _packed.CompareTo(other._packed);
}
