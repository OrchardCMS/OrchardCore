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

    public string Location { get; set; }
    public string Source { get; set; }
    public string ShapeType { get; set; }
    public string DefaultPosition { get; set; }
    public AlternatesCollection Alternates { get; set; }
    public AlternatesCollection Wrappers { get; set; }

    public bool IsLayoutZone()
    {
        return Location.StartsWith('/');
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
