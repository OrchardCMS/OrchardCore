namespace OrchardCore.DisplayManagement.Theming;

/// <summary>
/// Represents a result for a selected theme.
/// </summary>
public class ThemeSelectorResult
{
    /// <summary>
    /// Gets or sets the priority for the selected theme.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets the theme name.
    /// </summary>
    /// <remarks>This is the theme identifier. By default this is also the theme name, but if a custom identifier has been specified, please use the identifier.</remarks>
    public string ThemeName { get; set; }
}
