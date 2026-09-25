using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a default implementation for <see cref="ILocalizationService"/>.
/// </summary>
public class DefaultLocalizationService : ILocalizationService
{
    // CultureInfo.InstalledUICulture.Name is empty when the OS/runtime has no configured
    // UI culture (e.g. invariant globalization mode, or a POSIX "C"/"C.UTF-8" locale, common
    // on Linux CI runners and containers) - falling back to "en" keeps GetSupportedCulturesAsync
    // from ever returning an empty culture name, which callers (e.g.
    // CultureInfo.GetCultureInfo(cultureName)) treat as invalid input and throw on.
    private static readonly string s_installedOrDefaultCulture =
        string.IsNullOrEmpty(CultureInfo.InstalledUICulture.Name) ? "en" : CultureInfo.InstalledUICulture.Name;

    private static readonly Task<string> s_defaultCulture = Task.FromResult(s_installedOrDefaultCulture);
    private static readonly Task<string[]> s_supportedCultures = Task.FromResult(new[] { s_installedOrDefaultCulture });

    /// <inheritdocs />
    public bool FallBackToParentCultures => true;

    /// <inheritdocs />
    public Task<string> GetDefaultCultureAsync() => s_defaultCulture;

    /// <inheritdocs />
    public Task<string[]> GetSupportedCulturesAsync() => s_supportedCultures;

    public IEnumerable<CultureInfo> GetAllCulturesAndAliases() => CultureInfo.GetCultures(CultureTypes.AllCultures);
}
