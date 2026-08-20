using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a default implementation for <see cref="ILocalizationService"/>.
/// </summary>
public class DefaultLocalizationService : ILocalizationService
{
    private static readonly Task<string> s_defaultCulture = Task.FromResult(CultureInfo.InstalledUICulture.Name);
    private static readonly Task<string[]> s_supportedCultures = Task.FromResult(new[] { CultureInfo.InstalledUICulture.Name });

    /// <inheritdocs />
    public bool FallBackToParentCultures => true;

    /// <inheritdocs />
    public Task<string> GetDefaultCultureAsync() => s_defaultCulture;

    /// <inheritdocs />
    public Task<string[]> GetSupportedCulturesAsync() => s_supportedCultures;

    public IEnumerable<CultureInfo> GetAllCulturesAndAliases() => CultureInfo.GetCultures(CultureTypes.AllCultures);
}
