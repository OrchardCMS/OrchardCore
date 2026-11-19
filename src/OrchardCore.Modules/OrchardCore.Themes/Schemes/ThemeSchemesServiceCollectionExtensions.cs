using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// Registers the themes that support schemes.
/// </summary>
public static class ThemeSchemesServiceCollectionExtensions
{
    /// <summary>
    /// Makes a theme support schemes: adds its id to <see cref="ThemeSchemesOptions.Themes"/> and configures its
    /// <see cref="ThemeSchemeOptions"/>, named after the theme. Calling it again for the same theme, whatever the case
    /// of the id, adds to its configuration. Call it from the theme's startup, and add <c>OrchardCore.Themes.Schemes</c>
    /// to the dependencies of the theme's manifest.
    /// </summary>
    /// <param name="services">The services of the theme's startup.</param>
    /// <param name="themeId">The id of the theme, the id of its extension (e.g. <c>MyTheme</c>).</param>
    /// <param name="configure">Configures the theme's options; every option has a default.</param>
    public static IServiceCollection AddThemeSchemes(this IServiceCollection services, string themeId, Action<ThemeSchemeOptions> configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrEmpty(themeId);

        services.AddOptions();
        services.Configure<ThemeSchemesOptions>(options => options.Themes.Add(themeId));
        services.AddSingleton<IConfigureOptions<ThemeSchemeOptions>>(new ConfigureThemeSchemeOptions(themeId, configure));

        return services;
    }

    // Applies to the options of the theme whatever the case of the name they are read with, so that two registrations
    // of a theme spelled differently combine.
    private sealed class ConfigureThemeSchemeOptions : IConfigureNamedOptions<ThemeSchemeOptions>
    {
        private readonly string _themeId;
        private readonly Action<ThemeSchemeOptions> _configure;

        public ConfigureThemeSchemeOptions(string themeId, Action<ThemeSchemeOptions> configure)
        {
            _themeId = themeId;
            _configure = configure;
        }

        public void Configure(string name, ThemeSchemeOptions options)
        {
            if (!string.Equals(name, _themeId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            options.ThemeId = name;
            _configure?.Invoke(options);
        }

        public void Configure(ThemeSchemeOptions options)
            => Configure(Options.DefaultName, options);
    }
}
