using OrchardCore.DisplayManagement.Shapes;

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

    public string Location { get; init; }
    public string Source { get; init; }
    public string ShapeType { get; init; }
    public string DefaultPosition { get; init; }
    public AlternatesCollection Alternates { get; init; }
    public AlternatesCollection Wrappers { get; init; }

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

        return new PlacementInfo { Location = location };
    }

    /// <summary>
    /// Creates a new <see cref="PlacementInfo"/> by merging this instance with additional properties.
    /// Returns this instance if no properties need to be changed.
    /// </summary>
    public PlacementInfo WithSource(string source)
    {
        if (Source == source)
        {
            return this;
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
        var needsLocation = Location == null && location != null;
        var needsDefaultPosition = DefaultPosition == null && defaultPosition != null;

        if (!needsLocation && !needsDefaultPosition)
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
    {
        return Location?.StartsWith('/') == true;
    }

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
        if (string.IsNullOrEmpty(Location))
        {
            return [];
        }

        string zones;
        var location = Location;

        // Strip the Layout marker.
        if (IsLayoutZone())
        {
            location = location[1..];
        }

        var firstDelimiter = location.IndexOfAny(_delimiters);
        if (firstDelimiter == -1)
        {
            zones = location;
        }
        else
        {
            zones = location[..firstDelimiter];
        }

        return zones.Split('.');
    }

    public string GetPosition()
    {
        if (string.IsNullOrEmpty(Location))
        {
            return DefaultPosition ?? "";
        }

        var contentDelimiter = Location.IndexOf(PositionDelimiter);
        if (contentDelimiter == -1)
        {
            return DefaultPosition ?? "";
        }

        var secondDelimiter = Location.IndexOfAny(_delimiters, contentDelimiter + 1);
        if (secondDelimiter == -1)
        {
            return Location[(contentDelimiter + 1)..];
        }

        return Location[(contentDelimiter + 1)..secondDelimiter];
    }

    public string GetTab()
    {
        if (string.IsNullOrEmpty(Location))
        {
            return "";
        }

        var tabDelimiter = Location.IndexOf(TabDelimiter);
        if (tabDelimiter == -1)
        {
            return "";
        }

        var nextDelimiter = Location.IndexOfAny(_delimiters, tabDelimiter + 1);
        if (nextDelimiter == -1)
        {
            return Location[(tabDelimiter + 1)..];
        }

        return Location[(tabDelimiter + 1)..nextDelimiter];
    }

    /// <summary>
    /// Extracts the group information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12@search.
    /// </summary>
    public string GetGroup()
    {
        if (string.IsNullOrEmpty(Location))
        {
            return null;
        }

        var groupDelimiter = Location.IndexOf(GroupDelimiter);
        if (groupDelimiter == -1)
        {
            return null;
        }

        var nextDelimiter = Location.IndexOfAny(_delimiters, groupDelimiter + 1);
        if (nextDelimiter == -1)
        {
            return Location[(groupDelimiter + 1)..];
        }

        return Location[(groupDelimiter + 1)..nextDelimiter];
    }

    /// <summary>
    /// Extracts the card information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12%search.
    /// </summary>
    public string GetCard()
    {
        if (string.IsNullOrEmpty(Location))
        {
            return null;
        }

        var cardDelimiter = Location.IndexOf(CardDelimiter);
        if (cardDelimiter == -1)
        {
            return null;
        }

        var nextDelimiter = Location.IndexOfAny(_delimiters, cardDelimiter + 1);
        if (nextDelimiter == -1)
        {
            return Location[(cardDelimiter + 1)..];
        }

        return Location[(cardDelimiter + 1)..nextDelimiter];
    }

    /// <summary>
    /// Extracts the column information from a location string, or <c>null</c> if it is not present.
    /// e.g., Content:12!search.
    /// </summary>
    public string GetColumn()
    {
        if (string.IsNullOrEmpty(Location))
        {
            return null;
        }

        var colDelimiter = Location.IndexOf(ColumnDelimiter);
        if (colDelimiter == -1)
        {
            return null;
        }

        var nextDelimiter = Location.IndexOfAny(_delimiters, colDelimiter + 1);
        if (nextDelimiter == -1)
        {
            return Location[(colDelimiter + 1)..];
        }

        return Location[(colDelimiter + 1)..nextDelimiter];
    }
}
