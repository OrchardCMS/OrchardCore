using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrchardCore.Themes.Schemes;

public static class ThemeSchemesServiceCollectionExtensions
{
    /// <summary>
    /// Makes a theme support schemes: registers its <see cref="IThemeSchemeProvider"/> and adds its theme id to
    /// <see cref="ThemeSchemesOptions.Themes"/>. Call it from the theme's startup, and add
    /// <c>OrchardCore.Themes.Schemes</c> to the dependencies of the theme's manifest.
    /// </summary>
    public static IServiceCollection AddThemeSchemes<TProvider>(this IServiceCollection services)
        where TProvider : class, IThemeSchemeProvider
    {
        services.TryAddSingleton<TProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IThemeSchemeProvider, TProvider>(serviceProvider => serviceProvider.GetRequiredService<TProvider>()));
        services.AddOptions<ThemeSchemesOptions>()
            .Configure<TProvider>((options, provider) => options.Themes.Add(provider.ThemeId));

        return services;
    }
}
