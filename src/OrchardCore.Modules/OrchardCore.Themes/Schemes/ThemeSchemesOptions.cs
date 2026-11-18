namespace OrchardCore.Themes.Schemes;

/// <summary>
/// The themes that support schemes. Each call to
/// <see cref="ThemeSchemesServiceCollectionExtensions.AddThemeSchemes{TProvider}"/> adds the id of its theme.
/// </summary>
public sealed class ThemeSchemesOptions
{
    /// <summary>
    /// Gets the ids of the themes that support schemes. Theme ids are compared without regard to case.
    /// </summary>
    public ISet<string> Themes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
