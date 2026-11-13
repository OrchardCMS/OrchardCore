namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// Declares a placement location that a display driver sets with a value that isn't a compile-time constant, so that
/// the names of its tabs and cards can be discovered for translation.
/// </summary>
/// <remarks>
/// The tab and card names of constant locations are discovered at build time. Declare the location on the driver
/// that sets it, for instance one returned by a virtual property, by referencing the same constant. A derived driver
/// that sets another location declares its own. Names that come from data are listed by an
/// <c>ILocalizationDataProvider</c> under the <see cref="TabAndCardNames.DataLocalizationContext"/> context.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class PlacementLocationAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlacementLocationAttribute"/> class.
    /// </summary>
    /// <param name="location">The placement location, such as <c>Content:5#Settings</c>.</param>
    public PlacementLocationAttribute(string location)
    {
        Location = location;
    }

    /// <summary>
    /// Gets the placement location.
    /// </summary>
    public string Location { get; }
}
