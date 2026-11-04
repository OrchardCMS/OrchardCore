using System.ComponentModel;

namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// Lists the placement locations, tabs and cards that a type of the assembly sets. It is generated at build time.
/// </summary>
/// <remarks>
/// The generator reads the constant locations of <c>ShapeResult.Location()</c>, the names of
/// <see cref="PlacementLocationBuilder.Tab(string, string)"/> and <see cref="PlacementLocationBuilder.Card(string, string)"/>,
/// and the locations declared with <see cref="PlacementLocationAttribute"/>, and keeps those with a tab or a card.
/// </remarks>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PlacementCatalogAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementCatalogAttribute"/> class.
    /// </summary>
    /// <param name="declaringType">
    /// The type that sets the locations, its generic type definition when it is generic.
    /// </param>
    public PlacementCatalogAttribute(Type declaringType)
    {
        DeclaringType = declaringType;
    }

    /// <summary>
    /// Gets the type that sets the locations.
    /// </summary>
    public Type DeclaringType { get; }

    /// <summary>
    /// Gets or sets the placement locations with a tab or a card.
    /// </summary>
    public string[] Locations { get; set; } = [];

    /// <summary>
    /// Gets or sets the tab names set with <see cref="PlacementLocationBuilder.Tab(string, string)"/>.
    /// </summary>
    public string[] Tabs { get; set; } = [];

    /// <summary>
    /// Gets or sets the card names set with <see cref="PlacementLocationBuilder.Card(string, string)"/>.
    /// </summary>
    public string[] Cards { get; set; } = [];
}
