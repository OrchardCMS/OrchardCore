using Cysharp.Text;

namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// Represents pre-parsed grouping metadata for Tab, Card, or Column placements.
/// This avoids repeated string parsing during rendering.
/// </summary>
public readonly struct GroupingMetadata : IEquatable<GroupingMetadata>
{
    /// <summary>
    /// Gets an empty grouping metadata instance.
    /// </summary>
    public static readonly GroupingMetadata Empty;

    /// <summary>
    /// Gets the name of the grouping (e.g., "Settings", "Details", "Left").
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the position modifier for ordering this group relative to other groups.
    /// This is the value after the ';' delimiter.
    /// </summary>
    public string Position { get; }

    /// <summary>
    /// Gets the width modifier for columns (e.g., "9" for col-md-9, or "lg-9" for col-lg-9).
    /// This is the value after the '_' delimiter. Only applicable for Column groupings.
    /// </summary>
    public string Width { get; }

    /// <summary>
    /// Gets whether this grouping metadata has a value.
    /// </summary>
    public bool HasValue => !string.IsNullOrEmpty(Name);

    /// <summary>
    /// Creates a new grouping metadata with parsed values.
    /// </summary>
    public GroupingMetadata(string name, string position = null, string width = null)
    {
        Name = name;
        Position = position;
        Width = width;
    }

    /// <summary>
    /// Parses a Tab or Card string into a <see cref="GroupingMetadata"/>.
    /// Format: "Name" or "Name;position".
    /// </summary>
    /// <param name="value">The raw string value from the placement.</param>
    /// <returns>A parsed <see cref="GroupingMetadata"/> instance.</returns>
    public static GroupingMetadata ParseTabOrCard(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Empty;
        }

        var positionIndex = value.IndexOf(';');
        if (positionIndex == -1)
        {
            return new GroupingMetadata(value);
        }

        var name = value[..positionIndex];
        var position = value[(positionIndex + 1)..];
        return new GroupingMetadata(name, position);
    }

    /// <summary>
    /// Parses a Column string into a <see cref="GroupingMetadata"/>.
    /// Format: "Name", "Name_width", "Name;position", or "Name_width;position" (or "Name;position_width").
    /// </summary>
    /// <param name="value">The raw string value from the placement.</param>
    /// <returns>A parsed <see cref="GroupingMetadata"/> instance.</returns>
    public static GroupingMetadata ParseColumn(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return Empty;
        }

        var widthIndex = value.IndexOf('_');
        var positionIndex = value.IndexOf(';');

        // No modifiers.
        if (widthIndex == -1 && positionIndex == -1)
        {
            return new GroupingMetadata(value);
        }

        string name;
        string position;
        string width;

        // Only position modifier: "Name;position".
        if (widthIndex == -1)
        {
            name = value[..positionIndex];
            position = value[(positionIndex + 1)..];
            return new GroupingMetadata(name, position);
        }

        // Only width modifier: "Name_width".
        if (positionIndex == -1)
        {
            name = value[..widthIndex];
            width = value[(widthIndex + 1)..];
            return new GroupingMetadata(name, null, width);
        }

        // Both modifiers present.
        // Determine which comes first to extract the name correctly.
        var firstModifierIndex = Math.Min(widthIndex, positionIndex);
        name = value[..firstModifierIndex];

        // Width comes before position: "Name_width;position".
        if (widthIndex < positionIndex)
        {
            width = value[(widthIndex + 1)..positionIndex];
            position = value[(positionIndex + 1)..];
        }
        // Position comes before width: "Name;position_width".
        else
        {
            position = value[(positionIndex + 1)..widthIndex];
            width = value[(widthIndex + 1)..];
        }

        return new GroupingMetadata(name, position, width);
    }

    public bool Equals(GroupingMetadata other)
        => Name == other.Name && Position == other.Position && Width == other.Width;

    public override bool Equals(object obj)
        => obj is GroupingMetadata other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Name, Position, Width);

    public static bool operator ==(GroupingMetadata left, GroupingMetadata right)
        => left.Equals(right);

    public static bool operator !=(GroupingMetadata left, GroupingMetadata right)
        => !left.Equals(right);

    /// <summary>
    /// Implicit conversion from string. Parses as a Tab/Card format (Name;position).
    /// For Column format with width, use <see cref="ParseColumn"/> explicitly.
    /// </summary>
    public static implicit operator GroupingMetadata(string value)
        => ParseTabOrCard(value);

    /// <summary>
    /// Implicit conversion to string. Returns the string representation of this grouping.
    /// </summary>
    public static implicit operator string(GroupingMetadata metadata)
        => metadata.ToString();

    /// <summary>
    /// Returns the string representation for backward compatibility.
    /// For Tab/Card: "Name" or "Name;position".
    /// For Column: "Name", "Name_width", "Name;position", or "Name_width;position".
    /// </summary>
    public override string ToString()
    {
        if (!HasValue)
        {
            return null;
        }

        if (string.IsNullOrEmpty(Position) && string.IsNullOrEmpty(Width))
        {
            return Name;
        }

        using var sb = ZString.CreateStringBuilder();
        sb.Append(Name);

        if (!string.IsNullOrEmpty(Width))
        {
            sb.Append('_');
            sb.Append(Width);
        }

        if (!string.IsNullOrEmpty(Position))
        {
            sb.Append(';');
            sb.Append(Position);
        }

        return sb.ToString();
    }
}
