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
    private PlacementInfo _cachedPlacementInfo;

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
        _cachedPlacementInfo = null;
        return this;
    }

    /// <summary>
    /// Marks this placement as targeting a layout zone (prefixed with <c>/</c>).
    /// </summary>
    public PlacementLocationBuilder AsLayoutZone()
    {
        _isLayoutZone = true;
        _cachedPlacementInfo = null;
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
        _cachedPlacementInfo = null;
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
        _cachedPlacementInfo = null;
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
        _cachedPlacementInfo = null;
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
        _cachedPlacementInfo = null;
        return this;
    }

    /// <summary>
    /// Builds a <see cref="PlacementInfo"/> instance from the current builder state.
    /// </summary>
    /// <returns>A new <see cref="PlacementInfo"/> with the configured location.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Zone"/> has not been called.</exception>
    public PlacementInfo Build()
    {
        if (_cachedPlacementInfo != null)
        {
            return _cachedPlacementInfo;
        }

        if (string.IsNullOrEmpty(_zone))
        {
            throw new InvalidOperationException("Zone is required. Call Zone() before calling Build().");
        }

        // Create GroupingMetadata directly without string allocation.
        var tabGrouping = !string.IsNullOrEmpty(_tabName)
            ? new GroupingMetadata(_tabName, _tabPosition)
            : GroupingMetadata.Empty;

        var cardGrouping = !string.IsNullOrEmpty(_cardName)
            ? new GroupingMetadata(_cardName, _cardPosition)
            : GroupingMetadata.Empty;

        var columnGrouping = !string.IsNullOrEmpty(_columnName)
            ? new GroupingMetadata(_columnName, _columnPosition, _columnWidth)
            : GroupingMetadata.Empty;

        // Split zones only once - use the zone string directly if no dots.
        var zones = _zone.Contains('.')
            ? _zone.Split('.')
            : [_zone];

        // Use the internal constructor that doesn't require a location string.
        // The location string will be lazily computed by PlacementInfo.Location when needed.
        _cachedPlacementInfo = new PlacementInfo(
            source: null,
            shapeType: null,
            defaultPosition: null,
            alternates: null,
            wrappers: null,
            zones: zones,
            position: _position,
            tabGrouping: tabGrouping,
            group: _group,
            cardGrouping: cardGrouping,
            columnGrouping: columnGrouping,
            isLayoutZone: _isLayoutZone);

        return _cachedPlacementInfo;
    }

    /// <summary>
    /// Returns the placement location string.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Zone"/> has not been called.</exception>
    public override string ToString()
        => Build().Location;

    /// <summary>
    /// Implicit conversion to <see cref="PlacementInfo"/> for convenience.
    /// </summary>
    public static implicit operator PlacementInfo(PlacementLocationBuilder builder)
        => builder?.Build();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementLocationBuilder builder)
        => builder?.ToString();
}
