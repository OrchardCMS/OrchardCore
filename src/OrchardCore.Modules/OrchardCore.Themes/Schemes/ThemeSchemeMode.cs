namespace OrchardCore.Themes.Schemes;

/// <summary>
/// The color mode a scheme's tokens are written for.
/// </summary>
public enum ThemeSchemeMode
{
    /// <summary>
    /// The light mode: the rules on <c>[{AttributeName}="{id}"]</c>, which also hold the tokens both modes share.
    /// </summary>
    Light,

    /// <summary>
    /// The dark mode: the rules on <c>{DarkModeSelector}[{AttributeName}="{id}"]</c>, which hold what changes in dark
    /// mode only.
    /// </summary>
    Dark,
}
