using System.Collections.Concurrent;

namespace OrchardCore.DisplayManagement.Descriptors;

public class PlacementInfo
{
    public const string HiddenLocation = "-";

    public const char PositionDelimiter = ':';

    public const char TabDelimiter = '#';

    public const char GroupDelimiter = '@';

    public const char CardDelimiter = '%';

    public const char ColumnDelimiter = '|';

    private static readonly char[] _delimiters = [PositionDelimiter, TabDelimiter, GroupDelimiter, CardDelimiter, ColumnDelimiter];

    /// <summary>
    /// A shared instance representing a hidden placement.
    /// </summary>
    public static readonly PlacementInfo Hidden = new(HiddenLocation);

    /// <summary>
    /// A shared instance representing an empty placement with no properties set.
    /// </summary>
    public static readonly PlacementInfo Empty = new();

    /// <summary>
    /// Cache for commonly used PlacementInfo instances with only Location set.
    /// </summary>
    private static readonly ConcurrentDictionary<string, PlacementInfo> _locationCache = new(StringComparer.OrdinalIgnoreCase);

    private string _location;

    /// <summary>
    /// Gets the location string. This is lazily computed from the parsed components if not explicitly set.
    /// </summary>
    public string Location => _location ??= BuildLocationString();

    public string Source { get; }
    public string ShapeType { get; }
    public string DefaultPosition { get; }
    public string[] Alternates { get; }
    public string[] Wrappers { get; }

    /// <summary>
    /// Returns the list of zone names.
    /// e.g., <code>Content.Metadata:1</code> will return 'Content', 'Metadata'.
    /// </summary>
    public string[] Zones { get; }

    /// <summary>
    /// Gets the position of the shape within the zone.
    /// Returns the explicit position if set, otherwise falls back to <see cref="DefaultPosition"/>, or empty string.
    /// </summary>
    public string Position => _position ?? DefaultPosition ?? "";

    /// <summary>
    /// Gets the tab information from the placement, or empty string if not present.
    /// </summary>
    public string Tab => _tab ?? "";

    /// <summary>
    /// Gets the group information from the placement, or <c>null</c> if not present.
    /// e.g., Content:12@search.
    /// </summary>
    public string Group { get; }

    /// <summary>
    /// Gets the card information from the placement, or <c>null</c> if not present.
    /// e.g., Content:12%search.
    /// </summary>
    public string Card { get; }

    /// <summary>
    /// Gets the column information from the placement, or <c>null</c> if not present.
    /// e.g., Content:12|search.
    /// </summary>
    public string Column { get; }

    private readonly string _position;
    private readonly string _tab;
    private readonly bool _isLayoutZone;

    public PlacementInfo()
    {
        Zones = [];
    }

    public PlacementInfo(
        string location,
        string source = null,
        string shapeType = null,
        string defaultPosition = null,
        string[] alternates = null,
        string[] wrappers = null)
    {
        _location = location;
        Source = source;
        ShapeType = shapeType;
        DefaultPosition = defaultPosition;
        Alternates = alternates;
        Wrappers = wrappers;

        if (!string.IsNullOrEmpty(location))
        {
            _isLayoutZone = location[0] == '/';
            ParseLocation(location, out var zones, out _position, out _tab, out var group, out var card, out var column);
            Zones = zones;
            Group = group;
            Card = card;
            Column = column;
        }
        else
        {
            Zones = [];
        }
    }

    public PlacementInfo(
        string location,
        string source,
        string shapeType,
        string defaultPosition,
        string[] alternates,
        string[] wrappers,
        string[] zones,
        string position,
        string tab,
        string group,
        string card,
        string column)
        : this(location, source, shapeType, defaultPosition, alternates, wrappers, zones, position, tab, group, card, column,
            isLayoutZone: !string.IsNullOrEmpty(location) && location[0] == '/')
    {
    }

    /// <summary>
    /// Creates a PlacementInfo with pre-parsed components. The location string will be lazily computed.
    /// </summary>
    internal PlacementInfo(
        string source,
        string shapeType,
        string defaultPosition,
        string[] alternates,
        string[] wrappers,
        string[] zones,
        string position,
        string tab,
        string group,
        string card,
        string column,
        bool isLayoutZone)
    {
        // _location is left null - will be computed lazily via BuildLocationString()
        Source = source;
        ShapeType = shapeType;
        DefaultPosition = defaultPosition;
        Alternates = alternates;
        Wrappers = wrappers;
        Zones = zones ?? [];
        _position = position;
        _tab = tab;
        Group = group;
        Card = card;
        Column = column;
        _isLayoutZone = isLayoutZone;
    }

    private PlacementInfo(
        string location,
        string source,
        string shapeType,
        string defaultPosition,
        string[] alternates,
        string[] wrappers,
        string[] zones,
        string position,
        string tab,
        string group,
        string card,
        string column,
        bool isLayoutZone)
    {
        _location = location;
        Source = source;
        ShapeType = shapeType;
        DefaultPosition = defaultPosition;
        Alternates = alternates;
        Wrappers = wrappers;
        Zones = zones ?? [];
        _position = position;
        _tab = tab;
        Group = group;
        Card = card;
        Column = column;
        _isLayoutZone = isLayoutZone;
    }

    /// <summary>
    /// Creates a new <see cref="PlacementInfo"/> with the specified location.
    /// Returns a cached instance for common locations.
    /// </summary>
    public static PlacementInfo FromLocation(string location)
    {
        if (string.IsNullOrEmpty(location))
        {
            return null;
        }

        if (location == HiddenLocation)
        {
            return Hidden;
        }

        return _locationCache.GetOrAdd(location, static loc => new PlacementInfo(loc));
    }

    /// <summary>
    /// Returns a new PlacementInfo instance with the specified source value, or the current instance if the source is
    /// unchanged.
    /// </summary>
    /// <param name="source">The source identifier to associate with the returned PlacementInfo instance. Can be null or empty.</param>
    /// <returns>A PlacementInfo instance with the specified source value. Returns the current instance if the source is
    /// unchanged.</returns>
    public PlacementInfo WithSource(string source)
    {
        if (Source == source)
        {
            return this;
        }

        return new PlacementInfo(_location, source, ShapeType, DefaultPosition, Alternates, Wrappers, Zones, _position, _tab, Group, Card, Column, _isLayoutZone);
    }

    /// <summary>
    /// Creates a new <see cref="PlacementInfo"/> by merging this instance with location and default position.
    /// Returns this instance if no properties need to be changed.
    /// </summary>
    public PlacementInfo WithDefaults(string location, string defaultPosition)
    {
        var locationChanged = _location != location && location != null && _location == null;
        var defaultPositionChanged = DefaultPosition != defaultPosition && defaultPosition != null && DefaultPosition == null;

        if (!locationChanged && !defaultPositionChanged)
        {
            return this;
        }

        var newLocation = _location ?? location;
        var newDefaultPosition = DefaultPosition ?? defaultPosition;

        // If the location is changing, we need to parse the new location to get the zones and other values.
        if (locationChanged)
        {
            return new PlacementInfo(newLocation, Source, ShapeType, newDefaultPosition, Alternates, Wrappers);
        }

        // Location is not changing, preserve the already-parsed values.
        return new PlacementInfo(_location, Source, ShapeType, newDefaultPosition, Alternates, Wrappers, Zones, _position, _tab, Group, Card, Column, _isLayoutZone);
    }

    /// <summary>
    /// Determines whether this placement targets a layout zone (location starts with '/').
    /// </summary>
    public bool IsLayoutZone()
        => _isLayoutZone;

    /// <summary>
    /// Determines whether this placement represents a hidden shape.
    /// </summary>
    public bool IsHidden()
        => Zones.Length == 0 || _location == HiddenLocation;

    /// <summary>
    /// Returns the location string for debugging purposes.
    /// </summary>
    public override string ToString()
        => Location ?? "(empty)";

    /// <summary>
    /// Returns the list of zone names.
    /// e.g., <code>Content.Metadata:1</code> will return 'Content', 'Metadata'.
    /// </summary>
    [Obsolete($"Use the {nameof(Zones)} property instead.")]
    public string[] GetZones()
        => Zones;

    [Obsolete($"Use the {nameof(Position)} property instead.")]
    public string GetPosition()
        => Position;

    [Obsolete($"Use the {nameof(Tab)} property instead.")]
    public string GetTab()
        => Tab;

    /// <summary>
    /// Extracts the group information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12@search.
    /// </summary>
    [Obsolete($"Use the {nameof(Group)} property instead.")]
    public string GetGroup()
        => Group;

    /// <summary>
    /// Extracts the card information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12%search.
    /// </summary>
    [Obsolete($"Use the {nameof(Card)} property instead.")]
    public string GetCard()
        => Card;

    /// <summary>
    /// Extracts the column information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12|search.
    /// </summary>
    [Obsolete($"Use the {nameof(Column)} property instead.")]
    public string GetColumn()
        => Column;

    /// <summary>
    /// Builds the location string from the parsed components.
    /// </summary>
    private string BuildLocationString()
    {
        if (Zones.Length == 0)
        {
            return null;
        }

        // Calculate the required length.
        var length = 0;

        if (_isLayoutZone)
        {
            length++; // '/'
        }

        // Zones joined by '.'
        for (var i = 0; i < Zones.Length; i++)
        {
            if (i > 0)
            {
                length++; // '.'
            }
            length += Zones[i].Length;
        }

        if (!string.IsNullOrEmpty(_position))
        {
            length += 1 + _position.Length; // ':' + position
        }

        if (!string.IsNullOrEmpty(_tab))
        {
            length += 1 + _tab.Length; // '#' + tab
        }

        if (!string.IsNullOrEmpty(Group))
        {
            length += 1 + Group.Length; // '@' + group
        }

        if (!string.IsNullOrEmpty(Card))
        {
            length += 1 + Card.Length; // '%' + card
        }

        if (!string.IsNullOrEmpty(Column))
        {
            length += 1 + Column.Length; // '|' + column
        }

        return string.Create(length, this, static (span, info) =>
        {
            var offset = 0;

            if (info._isLayoutZone)
            {
                span[offset++] = '/';
            }

            // Write zones joined by '.'
            for (var i = 0; i < info.Zones.Length; i++)
            {
                if (i > 0)
                {
                    span[offset++] = '.';
                }
                info.Zones[i].AsSpan().CopyTo(span[offset..]);
                offset += info.Zones[i].Length;
            }

            if (!string.IsNullOrEmpty(info._position))
            {
                span[offset++] = PositionDelimiter;
                info._position.AsSpan().CopyTo(span[offset..]);
                offset += info._position.Length;
            }

            if (!string.IsNullOrEmpty(info._tab))
            {
                span[offset++] = TabDelimiter;
                info._tab.AsSpan().CopyTo(span[offset..]);
                offset += info._tab.Length;
            }

            if (!string.IsNullOrEmpty(info.Group))
            {
                span[offset++] = GroupDelimiter;
                info.Group.AsSpan().CopyTo(span[offset..]);
                offset += info.Group.Length;
            }

            if (!string.IsNullOrEmpty(info.Card))
            {
                span[offset++] = CardDelimiter;
                info.Card.AsSpan().CopyTo(span[offset..]);
                offset += info.Card.Length;
            }

            if (!string.IsNullOrEmpty(info.Column))
            {
                span[offset++] = ColumnDelimiter;
                info.Column.AsSpan().CopyTo(span[offset..]);
            }
        });
    }

    private static void ParseLocation(string location, out string[] zones, out string position, out string tab, out string group, out string card, out string column)
    {
        position = null;
        tab = null;
        group = null;
        card = null;
        column = null;

        if (location[0] == '/')
        {
            location = location[1..];
        }

        var firstDelimiter = location.IndexOfAny(_delimiters);
        string zonesString;
        if (firstDelimiter == -1)
        {
            zonesString = location;
        }
        else
        {
            zonesString = location[..firstDelimiter];
        }
        zones = zonesString.Split('.');

        var positionDelimiter = location.IndexOf(PositionDelimiter);
        if (positionDelimiter != -1)
        {
            var secondDelimiter = location.IndexOfAny(_delimiters, positionDelimiter + 1);
            if (secondDelimiter == -1)
            {
                position = location[(positionDelimiter + 1)..];
            }
            else
            {
                position = location[(positionDelimiter + 1)..secondDelimiter];
            }
        }

        var tabDelimiter = location.IndexOf(TabDelimiter);
        if (tabDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, tabDelimiter + 1);
            if (nextDelimiter == -1)
            {
                tab = location[(tabDelimiter + 1)..];
            }
            else
            {
                tab = location[(tabDelimiter + 1)..nextDelimiter];
            }
        }

        var groupDelimiter = location.IndexOf(GroupDelimiter);
        if (groupDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, groupDelimiter + 1);
            if (nextDelimiter == -1)
            {
                group = location[(groupDelimiter + 1)..];
            }
            else
            {
                group = location[(groupDelimiter + 1)..nextDelimiter];
            }
        }

        var cardDelimiter = location.IndexOf(CardDelimiter);
        if (cardDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, cardDelimiter + 1);
            if (nextDelimiter == -1)
            {
                card = location[(cardDelimiter + 1)..];
            }
            else
            {
                card = location[(cardDelimiter + 1)..nextDelimiter];
            }
        }

        var colDelimiter = location.IndexOf(ColumnDelimiter);
        if (colDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, colDelimiter + 1);
            if (nextDelimiter == -1)
            {
                column = location[(colDelimiter + 1)..];
            }
            else
            {
                column = location[(colDelimiter + 1)..nextDelimiter];
            }
        }
    }
}
