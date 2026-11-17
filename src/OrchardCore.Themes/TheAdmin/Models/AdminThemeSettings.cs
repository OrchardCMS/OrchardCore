namespace OrchardCore.Themes.TheAdmin.Models;

/// <summary>
/// The site settings of the admin theme.
/// </summary>
public sealed class AdminThemeSettings
{
    /// <summary>
    /// The id of the scheme of the admin for the users who have not picked their own. The built-in default scheme
    /// when empty or unknown. See <see cref="AdminSchemes"/>.
    /// </summary>
    public string DefaultScheme { get; set; }
}
