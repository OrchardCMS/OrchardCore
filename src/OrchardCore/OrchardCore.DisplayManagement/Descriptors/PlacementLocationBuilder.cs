using System.Text;

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
    internal string _zone;
    internal string _position;
    internal string _tab;
    internal string _group;
    internal string _card;
    internal string _column;
    internal bool _isLayoutZone;

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
    /// <returns>A builder to optionally add groupings (Tab, Card, Column).</returns>
    public PlacementZoneBuilder Zone(string zone, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(zone);
        _zone = zone;
        _position = position;
        return new PlacementZoneBuilder(this);
    }

    /// <summary>
    /// Builds the location string from the configured values.
    /// </summary>
    /// <returns>The placement location string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Zone"/> has not been called.</exception>
    public string Build()
    {
        if (string.IsNullOrEmpty(_zone))
        {
            throw new InvalidOperationException("Zone is required. Call Zone() before Build().");
        }

        var sb = new StringBuilder();

        if (_isLayoutZone)
        {
            sb.Append('/');
        }

        sb.Append(_zone);

        if (!string.IsNullOrEmpty(_position))
        {
            sb.Append(PlacementInfo.PositionDelimiter);
            sb.Append(_position);
        }

        if (!string.IsNullOrEmpty(_tab))
        {
            sb.Append(PlacementInfo.TabDelimiter);
            sb.Append(_tab);
        }

        if (!string.IsNullOrEmpty(_group))
        {
            sb.Append(PlacementInfo.GroupDelimiter);
            sb.Append(_group);
        }

        if (!string.IsNullOrEmpty(_card))
        {
            sb.Append(PlacementInfo.CardDelimiter);
            sb.Append(_card);
        }

        if (!string.IsNullOrEmpty(_column))
        {
            sb.Append(PlacementInfo.ColumnDelimiter);
            sb.Append(_column);
        }

        return sb.ToString();
    }

    /// <inheritdoc/>
    public override string ToString()
        => Build();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementLocationBuilder builder)
        => builder?.Build();
}

/// <summary>
/// Provides grouping options after a zone has been configured.
/// From here, you can add a Tab, Card, Column, or Group.
/// </summary>
public sealed class PlacementZoneBuilder
{
    private readonly PlacementLocationBuilder _root;

    internal PlacementZoneBuilder(PlacementLocationBuilder root)
    {
        _root = root;
    }

    /// <summary>
    /// Marks this placement as targeting a layout zone (prefixed with <c>/</c>).
    /// </summary>
    public PlacementZoneBuilder AsLayoutZone()
    {
        _root._isLayoutZone = true;
        return this;
    }

    /// <summary>
    /// Assigns a group to the shape.
    /// </summary>
    /// <param name="name">The group name (e.g., <c>"search"</c>).</param>
    public PlacementZoneBuilder Group(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._group = name;
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
    /// <returns>A builder to optionally add a Card or Column inside this tab.</returns>
    public PlacementTabBuilder Tab(string name, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._tab = string.IsNullOrEmpty(position) ? name : $"{name};{position}";
        return new PlacementTabBuilder(_root);
    }

    /// <summary>
    /// Groups the shape into a card (without a containing tab).
    /// </summary>
    /// <param name="name">The card name (e.g., <c>"Details"</c>).</param>
    /// <param name="position">
    /// Optional. The position of this card relative to other cards.
    /// All shapes in the same card should use the same card position value.
    /// </param>
    /// <returns>A builder to optionally add a Column inside this card.</returns>
    public PlacementCardBuilder Card(string name, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._card = string.IsNullOrEmpty(position) ? name : $"{name};{position}";
        return new PlacementCardBuilder(_root);
    }

    /// <summary>
    /// Groups the shape into a column (without a containing tab or card).
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
    public PlacementColumnBuilder Column(string name, string position = null, string width = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        SetColumn(_root, name, position, width);
        return new PlacementColumnBuilder(_root);
    }

    /// <inheritdoc cref="PlacementLocationBuilder.Build"/>
    public string Build() => _root.Build();

    /// <inheritdoc/>
    public override string ToString() => Build();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementZoneBuilder builder)
        => builder?.Build();

    internal static void SetColumn(PlacementLocationBuilder root, string name, string position, string width)
    {
        var value = name;

        if (!string.IsNullOrEmpty(width))
        {
            value = $"{value}_{width}";
        }

        if (!string.IsNullOrEmpty(position))
        {
            value = $"{value};{position}";
        }

        root._column = value;
    }
}

/// <summary>
/// Provides nesting options after a tab has been configured.
/// From here, you can add a Card or Column inside the tab.
/// </summary>
public sealed class PlacementTabBuilder
{
    private readonly PlacementLocationBuilder _root;

    internal PlacementTabBuilder(PlacementLocationBuilder root)
    {
        _root = root;
    }

    /// <summary>
    /// Assigns a group to the shape.
    /// </summary>
    /// <param name="name">The group name.</param>
    public PlacementTabBuilder Group(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._group = name;
        return this;
    }

    /// <summary>
    /// Groups the shape into a card inside the current tab.
    /// </summary>
    /// <param name="name">The card name (e.g., <c>"Details"</c>).</param>
    /// <param name="position">
    /// Optional. The position of this card relative to other cards.
    /// All shapes in the same card should use the same card position value.
    /// </param>
    /// <returns>A builder to optionally add a Column inside this card.</returns>
    public PlacementCardBuilder Card(string name, string position = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._card = string.IsNullOrEmpty(position) ? name : $"{name};{position}";
        return new PlacementCardBuilder(_root);
    }

    /// <summary>
    /// Groups the shape into a column inside the current tab (without a containing card).
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
    public PlacementColumnBuilder Column(string name, string position = null, string width = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        PlacementZoneBuilder.SetColumn(_root, name, position, width);
        return new PlacementColumnBuilder(_root);
    }

    /// <inheritdoc cref="PlacementLocationBuilder.Build"/>
    public string Build() => _root.Build();

    /// <inheritdoc/>
    public override string ToString() => Build();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementTabBuilder builder)
        => builder?.Build();
}

/// <summary>
/// Provides nesting options after a card has been configured.
/// From here, you can add a Column inside the card.
/// </summary>
public sealed class PlacementCardBuilder
{
    private readonly PlacementLocationBuilder _root;

    internal PlacementCardBuilder(PlacementLocationBuilder root)
    {
        _root = root;
    }

    /// <summary>
    /// Assigns a group to the shape.
    /// </summary>
    /// <param name="name">The group name.</param>
    public PlacementCardBuilder Group(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._group = name;
        return this;
    }

    /// <summary>
    /// Groups the shape into a column inside the current card.
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
    public PlacementColumnBuilder Column(string name, string position = null, string width = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        PlacementZoneBuilder.SetColumn(_root, name, position, width);
        return new PlacementColumnBuilder(_root);
    }

    /// <inheritdoc cref="PlacementLocationBuilder.Build"/>
    public string Build() => _root.Build();

    /// <inheritdoc/>
    public override string ToString() => Build();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementCardBuilder builder)
        => builder?.Build();
}

/// <summary>
/// Terminal builder after a column has been configured.
/// No further nesting is available.
/// </summary>
public sealed class PlacementColumnBuilder
{
    private readonly PlacementLocationBuilder _root;

    internal PlacementColumnBuilder(PlacementLocationBuilder root)
    {
        _root = root;
    }

    /// <summary>
    /// Assigns a group to the shape.
    /// </summary>
    /// <param name="name">The group name.</param>
    public PlacementColumnBuilder Group(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        _root._group = name;
        return this;
    }

    /// <inheritdoc cref="PlacementLocationBuilder.Build"/>
    public string Build() => _root.Build();

    /// <inheritdoc/>
    public override string ToString() => Build();

    /// <summary>
    /// Implicit conversion to string for convenience.
    /// </summary>
    public static implicit operator string(PlacementColumnBuilder builder)
        => builder?.Build();
}
