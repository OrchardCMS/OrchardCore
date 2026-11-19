namespace OrchardCore.Themes.Schemes;

/// <summary>
/// The themes that support schemes. Each call to
/// <see cref="ThemeSchemesServiceCollectionExtensions.AddThemeSchemes(Microsoft.Extensions.DependencyInjection.IServiceCollection, string, Action{ThemeSchemeOptions})"/>
/// adds the id of its theme; the options of each theme are the <see cref="ThemeSchemeOptions"/> named after it.
/// </summary>
public sealed class ThemeSchemesOptions
{
    /// <summary>
    /// Gets the ids of the themes that support schemes. Theme ids are compared without regard to case; the first
    /// spelling registered is the one the schemes are stored under.
    /// </summary>
    public ISet<string> Themes { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
