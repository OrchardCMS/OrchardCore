namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// A fluent builder for constructing placement location strings.
/// </summary>
/// <remarks>
/// <para>
/// Location strings follow the format: <c>[/]Zone:position#tab@group%card|column</c>.
/// This builder provides a type-safe alternative to manually constructing these strings.
/// </para>
/// <para>
/// The rendering hierarchy is: <strong>Zone → Tab → Card → Column</strong>.
/// Each level can have its own position that controls the ordering of groups at that level.
/// The shape's position within the zone is set via
/// <see cref="PlacementLocationBuilder.Zone(string, string)"/> and determines where
/// the shape appears relative to other shapes in the same zone.
/// </para>
/// <example>
/// <code>
/// // Place in "Content" zone at position 5.
/// .Location(l => l.Zone("Content", "5"))
///
/// // Place in "Parameters" zone at position 5, inside the "Settings" tab (tab ordered first).
/// .Location(l => l.Zone("Parameters", "5").Tab("Settings", "1"))
///
/// // Full nesting: zone → tab → card → column.
/// .Location(l => l
///     .Zone("Parameters", "5")
///     .Tab("Settings", "1")
///     .Card("Details", "2")
///     .Column("Left", "3", width: "9"))
/// </code>
/// </example>
/// </remarks>
public sealed class PlacementLocationBuilder
{
    private string _zone;
    private string _position;
    private string _tabName;
    private string _tabPosition;
    private string _group;
    private string _cardName;
    private string _cardPosition;
    private string _columnName;
    private string _columnPosition;
    private string _columnWidth;
    private bool _isLayoutZone;
    private string _cachedResult;

    /// <summary>
    /// Sets the target zone and the shape's position within that zone.
    /// </summary>
    /// <param name="zone">
    /// The zone name (e.g., <c>"Content"</c>, <c>"Parts"</c>).
    /// Multiple zone levels can be specified using dot notation (e.g., <c>"Content.Metadata"</c>).
    /// </param>
    /// <param name="position">
    /// Optional. The position of the shape within the zone. Controls rendering order
    /// among shapes in the same zone.
    /// <list type="bullet">
    /// <item>Compared numerically: <c>"2"</c> comes before <c>"10"</c>.</item>
    /// <item>Dot notation for sub-positions: <c>"1.5"</c> is between <c>"1"</c> and <c>"2"</c>.</item>
    /// <item><c>"before"</c> places the shape at the very beginning of the zone.</item>
    /// <item><c>"after"</c> places the shape at the very end of the zone.</item>
    /// <item>No position (default) is treated as <c>"0"</c>.</item>
    /// </list>
    /// </param>
    /// <returns>This builder instance for method chaining.</returns>
    public PlacementLocationBuilder Zone(string zone, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(zone);
        _zone = zone;
        _position = position;
        _cachedResult = null;
        return this;
    }

    /// <summary>
    /// Marks this placement as targeting a layout zone (prefixed with <c>/</c>).
    /// </summary>
    public PlacementLocationBuilder AsLayoutZone()
    {
        _isLayoutZone = true;
        _cachedResult = null;
        return this;
    }

    /// <summary>
    /// Assigns a group to the shape.
    /// </summary>
    /// <param name="name">The group name (e.g., <c>"search"</c>).</param>
    public PlacementLocationBuilder Group(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _group = name;
        _cachedResult = null;
        return this;
    }

    /// <summary>
    /// Groups the shape into a tab. Tabs are the outermost grouping level.
    /// </summary>
    /// <param name="name">The tab name (e.g., <c>"Settings"</c>, <c>"Content"</c>).</param>
    /// <param name="position">
    /// Optional. The position of this tab relative to other tabs.
    /// All shapes in the same tab should use the same tab position value.
    /// </param>
    public PlacementLocationBuilder Tab(string name, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _tabName = name;
        _tabPosition = position;
        _cachedResult = null;
        return this;
    }

    /// <summary>
    /// Groups the shape into a card.
    /// </summary>
    /// <param name="name">The card name (e.g., <c>"Details"</c>).</param>
    /// <param name="position">
    /// Optional. The position of this card relative to other cards.
    /// All shapes in the same card should use the same card position value.
    /// </param>
    public PlacementLocationBuilder Card(string name, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _cardName = name;
        _cardPosition = position;
        _cachedResult = null;
        return this;
    }

    /// <summary>
    /// Groups the shape into a column.
    /// </summary>
    /// <param name="name">The column name (e.g., <c>"Left"</c>, <c>"Right"</c>).</param>
    /// <param name="position">
    /// Optional. The position of this column relative to other columns.
    /// All shapes in the same column should use the same column position value.
    /// </param>
    /// <param name="width">
    /// Optional. The Bootstrap grid column width (e.g., <c>"9"</c> for <c>col-md-9</c>,
    /// or <c>"lg-9"</c> for <c>col-lg-9</c>).
    /// </param>
    public PlacementLocationBuilder Column(string name, string position = null, string width = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _columnName = name;
        _columnPosition = position;
        _columnWidth = width;
        _cachedResult = null;
        return this;
    }

    /// <summary>
    /// Returns the placement location string. The result is cached until the builder is modified.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Zone"/> has not been called.</exception>
    public override string ToString()
    {
        if (_cachedResult != null)
        {
            return _cachedResult;
        }

        if (string.IsNullOrEmpty(_zone))
        {
            throw new InvalidOperationException("Zone is required. Call Zone() before calling ToString().");
        }

        // Pre-calculate total length to use string.Create and avoid StringBuilder allocation.
        var length = _zone.Length;

        if (_isLayoutZone)
        {
            length++;
        }

        if (!string.IsNullOrEmpty(_position))
        {
            length += 1 + _position.Length;
        }

        if (!string.IsNullOrEmpty(_tabName))
        {
            length += 1 + _tabName.Length;

            if (!string.IsNullOrEmpty(_tabPosition))
            {
                length += 1 + _tabPosition.Length;
            }
        }

        if (!string.IsNullOrEmpty(_group))
        {
            length += 1 + _group.Length;
        }

        if (!string.IsNullOrEmpty(_cardName))
        {
            length += 1 + _cardName.Length;

            if (!string.IsNullOrEmpty(_cardPosition))
            {
                length += 1 + _cardPosition.Length;
            }
        }

        if (!string.IsNullOrEmpty(_columnName))
        {
            length += 1 + _columnName.Length;

            if (!string.IsNullOrEmpty(_columnWidth))
            {
                length += 1 + _columnWidth.Length;
            }

            if (!string.IsNullOrEmpty(_columnPosition))
            {
                length += 1 + _columnPosition.Length;
            }
        }

        _cachedResult = string.Create(length, this, static (span, builder) =>
        {
            var offset = 0;

            if (builder._isLayoutZone)
            {
                span[offset++] = '/';
            }

            builder._zone.AsSpan().CopyTo(span[offset..]);
            offset += builder._zone.Length;

            if (!string.IsNullOrEmpty(builder._position))
            {
                span[offset++] = PlacementInfo.PositionDelimiter;
                builder._position.AsSpan().CopyTo(span[offset..]);
                offset += builder._position.Length;
            }

            if (!string.IsNullOrEmpty(builder._tabName))
            {
                span[offset++] = PlacementInfo.TabDelimiter;
                builder._tabName.AsSpan().CopyTo(span[offset..]);
                offset += builder._tabName.Length;

                if (!string.IsNullOrEmpty(builder._tabPosition))
                {
                    span[offset++] = ';';
                    builder._tabPosition.AsSpan().CopyTo(span[offset..]);
                    offset += builder._tabPosition.Length;
                }
            }

            if (!string.IsNullOrEmpty(builder._group))
            {
                span[offset++] = PlacementInfo.GroupDelimiter;
                builder._group.AsSpan().CopyTo(span[offset..]);
                offset += builder._group.Length;
            }

            if (!string.IsNullOrEmpty(builder._cardName))
            {
                span[offset++] = PlacementInfo.CardDelimiter;
                builder._cardName.AsSpan().CopyTo(span[offset..]);
                offset += builder._cardName.Length;

                if (!string.IsNullOrEmpty(builder._cardPosition))
                {
                    span[offset++] = ';';
                    builder._cardPosition.AsSpan().CopyTo(span[offset..]);
                    offset += builder._cardPosition.Length;
                }
            }

            if (!string.IsNullOrEmpty(builder._columnName))
            {
                span[offset++] = PlacementInfo.ColumnDelimiter;
                builder._columnName.AsSpan().CopyTo(span[offset..]);
                offset += builder._columnName.Length;

                if (!string.IsNullOrEmpty(builder._columnWidth))
                {
                    span[offset++] = '_';
                    builder._columnWidth.AsSpan().CopyTo(span[offset..]);
                    offset += builder._columnWidth.Length;
                }

                if (!string.IsNullOrEmpty(builder._columnPosition))
                {
                    span[offset++] = ';';
                    builder._columnPosition.AsSpan().CopyTo(span[offset..]);
                    offset += builder._columnPosition.Length;
                }
            }
        });

        return _cachedResult;
    }

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementLocationBuilder builder)
        => builder?.ToString();
}
