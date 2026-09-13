using System.Globalization;

namespace OrchardCore.Localization.Models;

/// <summary>
/// Represents an object to store the localization settings.
/// </summary>
public class LocalizationSettings
{
    // CultureInfo.InstalledUICulture.Name is empty when the OS/runtime has no configured UI
    // culture (e.g. invariant globalization mode, or a POSIX "C"/"C.UTF-8" locale, common on
    // Linux CI runners and containers) - falling back to "en" keeps these defaults from ever
    // persisting an empty culture name, which downstream consumers (e.g.
    // CultureInfo.GetCultureInfo(cultureName)) treat as invalid input and throw on.
    private static readonly string s_installedOrDefaultCulture =
        string.IsNullOrEmpty(CultureInfo.InstalledUICulture.Name) ? "en" : CultureInfo.InstalledUICulture.Name;

    private static readonly string[] s_defaultSupportedCultures = [s_installedOrDefaultCulture];

    /// <summary>
    /// Creates a new instance of the <see cref="LocalizationSettings"/>.
    /// </summary>
    public LocalizationSettings()
    {
        DefaultCulture = s_installedOrDefaultCulture;
        SupportedCultures = s_defaultSupportedCultures;
    }

    /// <summary>
    /// Gets or sets the default culture of the site.
    /// </summary>
    public string DefaultCulture { get; set; }

    /// <summary>
    /// Gets or sets all the supported cultures of the site. It also contains the default culture.
    /// </summary>
    public string[] SupportedCultures { get; set; }

    /// <summary>
    /// Gets or sets whether the culture could fall back to it's parent culture in case the current culture is not determined.
    /// </summary>
    public bool FallBackToParentCulture { get; set; }
}
