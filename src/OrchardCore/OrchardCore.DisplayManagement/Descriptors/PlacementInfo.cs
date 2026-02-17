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
    public static readonly PlacementInfo Hidden = new() { Location = HiddenLocation };

    /// <summary>
    /// A shared instance representing an empty placement with no properties set.
    /// </summary>
    public static readonly PlacementInfo Empty = new() { };

    /// <summary>
    /// Cache for commonly used PlacementInfo instances with only Location set.
    /// </summary>
    private static readonly ConcurrentDictionary<string, PlacementInfo> _locationCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Cache for commonly used PlacementInfo instances with Location and Source set.
    /// Uses a tuple key of (location, source).
    /// </summary>
    private static readonly ConcurrentDictionary<(string Location, string Source), PlacementInfo> _locationSourceCache = new();

    // Cached parsing results
    private string[] _zones;
    private string _position;
    private string _tab;
    private string _group;
    private string _card;
    private string _column;
    private bool _parsed;

    public string Location { get; init; }
    public string Source { get; init; }
    public string ShapeType { get; init; }
    public string DefaultPosition { get; init; }
    public string[] Alternates { get; init; }
    public string[] Wrappers { get; init; }

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

        // Cache PlacementInfo instances with only Location set to avoid repeated allocations
        return _locationCache.GetOrAdd(location, static loc => new PlacementInfo { Location = loc });
    }

    /// <summary>
    /// Creates a new <see cref="PlacementInfo"/> by merging this instance with additional properties.
    /// Returns this instance if no properties need to be changed.
    /// For simple cases where only Source changes on a cached location, returns a cached instance.
    /// </summary>
    public PlacementInfo WithSource(string source)
    {
        if (Source == source)
        {
            return this;
        }

        // If this is a cached location-only instance and we're just adding a source,
        // try to use a cached location+source instance
        if (ShapeType == null && DefaultPosition == null &&
            Alternates == null && Wrappers == null && Source == null &&
            !string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(source))
        {
            var cacheKey = (Location, source);
            return _locationSourceCache.GetOrAdd(cacheKey, static key => new PlacementInfo
            {
                Location = key.Location,
                Source = key.Source,
            });
        }

        return new PlacementInfo
        {
            Location = Location,
            Source = source,
            ShapeType = ShapeType,
            DefaultPosition = DefaultPosition,
            Alternates = Alternates,
            Wrappers = Wrappers,
        };
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

        return new PlacementInfo
        {
            Location = Location ?? location,
            Source = Source,
            ShapeType = ShapeType,
            DefaultPosition = DefaultPosition ?? defaultPosition,
            Alternates = Alternates,
            Wrappers = Wrappers,
        };
    }

    public bool IsLayoutZone()
        => !string.IsNullOrEmpty(Location) && Location[0] == '/';

    public bool IsHidden()
    {
        // If there are no placement or it's explicitly noop then its hidden.
        return string.IsNullOrEmpty(Location) || Location == HiddenLocation;
    }

    /// <summary>
    /// Returns the list of zone names.
    /// e.g.,. <code>Content.Metadata:1</code> will return 'Content', 'Metadata'.
    /// </summary>
    /// <returns></returns>
    public string[] GetZones()
    {
        if (_zones != null)
        {
            return _zones;
        }

        if (string.IsNullOrEmpty(Location))
        {
            return _zones = [];
        }

        EnsureParsed();
        return _zones;
    }

    public string GetPosition()
    {
        if (_parsed)
        {
            return _position ?? DefaultPosition ?? "";
        }

        if (string.IsNullOrEmpty(Location))
        {
            return DefaultPosition ?? "";
        }

        EnsureParsed();
        return _position ?? DefaultPosition ?? "";
    }

    public string GetTab()
    {
        if (_parsed)
        {
            return _tab ?? "";
        }

        if (string.IsNullOrEmpty(Location))
        {
            return "";
        }

        EnsureParsed();
        return _tab ?? "";
    }

    /// <summary>
    /// Extracts the group information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12@search.
    /// </summary>
    public string GetGroup()
    {
        if (_parsed)
        {
            return _group;
        }

        if (string.IsNullOrEmpty(Location))
        {
            return null;
        }

        EnsureParsed();
        return _group;
    }

    /// <summary>
    /// Extracts the card information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12%search.
    /// </summary>
    public string GetCard()
    {
        if (_parsed)
        {
            return _card;
        }

        if (string.IsNullOrEmpty(Location))
        {
            return null;
        }

        EnsureParsed();
        return _card;
    }

    /// <summary>
    /// Extracts the column information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12!search.
    /// </summary>
    public string GetColumn()
    {
        if (_parsed)
        {
            return _column;
        }

        if (string.IsNullOrEmpty(Location))
        {
            return null;
        }

        EnsureParsed();
        return _column;
    }

    private void EnsureParsed()
    {
        if (_parsed)
        {
            return;
        }

        var location = Location;

        // Strip the Layout marker.
        if (IsLayoutZone())
        {
            location = location[1..];
        }

        // Parse zones
        var firstDelimiter = location.IndexOfAny(_delimiters);
        string zones;
        if (firstDelimiter == -1)
        {
            zones = location;
        }
        else
        {
            zones = location[..firstDelimiter];
        }
        _zones = zones.Split('.');

        // Parse position
        var positionDelimiter = location.IndexOf(PositionDelimiter);
        if (positionDelimiter != -1)
        {
            var secondDelimiter = location.IndexOfAny(_delimiters, positionDelimiter + 1);
            if (secondDelimiter == -1)
            {
                _position = location[(positionDelimiter + 1)..];
            }
            else
            {
                _position = location[(positionDelimiter + 1)..secondDelimiter];
            }
        }

        // Parse tab
        var tabDelimiter = location.IndexOf(TabDelimiter);
        if (tabDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, tabDelimiter + 1);
            if (nextDelimiter == -1)
            {
                _tab = location[(tabDelimiter + 1)..];
            }
            else
            {
                _tab = location[(tabDelimiter + 1)..nextDelimiter];
            }
        }

        // Parse group
        var groupDelimiter = location.IndexOf(GroupDelimiter);
        if (groupDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, groupDelimiter + 1);
            if (nextDelimiter == -1)
            {
                _group = location[(groupDelimiter + 1)..];
            }
            else
            {
                _group = location[(groupDelimiter + 1)..nextDelimiter];
            }
        }

        // Parse card
        var cardDelimiter = location.IndexOf(CardDelimiter);
        if (cardDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, cardDelimiter + 1);
            if (nextDelimiter == -1)
            {
                _card = location[(cardDelimiter + 1)..];
            }
            else
            {
                _card = location[(cardDelimiter + 1)..nextDelimiter];
            }
        }

        // Parse column
        var colDelimiter = location.IndexOf(ColumnDelimiter);
        if (colDelimiter != -1)
        {
            var nextDelimiter = location.IndexOfAny(_delimiters, colDelimiter + 1);
            if (nextDelimiter == -1)
            {
                _column = location[(colDelimiter + 1)..];
            }
            else
            {
                _column = location[(colDelimiter + 1)..nextDelimiter];
            }
        }

        _parsed = true;
    }
}
