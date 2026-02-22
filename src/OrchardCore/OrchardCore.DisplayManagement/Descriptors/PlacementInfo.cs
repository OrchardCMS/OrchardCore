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

    public string Location { get; }
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
        Location = location;
        Source = source;
        ShapeType = shapeType;
        DefaultPosition = defaultPosition;
        Alternates = alternates;
        Wrappers = wrappers;

        if (!string.IsNullOrEmpty(location))
        {
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
    {
        Location = location;
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

        return new PlacementInfo(Location, source, ShapeType, DefaultPosition, Alternates, Wrappers, Zones, _position, _tab, Group, Card, Column);
    }

    /// <summary>
    /// Creates a new <see cref="PlacementInfo"/> by merging this instance with location and default position.
    /// Returns this instance if no properties need to be changed.
    /// </summary>
    public PlacementInfo WithDefaults(string location, string defaultPosition)
    {
        var locationChanged = Location != location && location != null && Location == null;
        var defaultPositionChanged = DefaultPosition != defaultPosition && defaultPosition != null && DefaultPosition == null;

        if (!locationChanged && !defaultPositionChanged)
        {
            return this;
        }

        var newLocation = Location ?? location;
        var newDefaultPosition = DefaultPosition ?? defaultPosition;

        // If the location is changing, we need to parse the new location to get the zones and other values.
        if (locationChanged)
        {
            return new PlacementInfo(newLocation, Source, ShapeType, newDefaultPosition, Alternates, Wrappers);
        }

        // Location is not changing, preserve the already-parsed values.
        return new PlacementInfo(newLocation, Source, ShapeType, newDefaultPosition, Alternates, Wrappers, Zones, _position, _tab, Group, Card, Column);
    }

    public bool IsLayoutZone()
        => !string.IsNullOrEmpty(Location) && Location[0] == '/';

    public bool IsHidden()
        => string.IsNullOrEmpty(Location) || Location == HiddenLocation;

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
