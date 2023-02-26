using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents a utility class to update the <see cref="RequestLocalizationOptions"/>.
/// </summary>
/// <remarks>
/// This is mainly used in the localization module to update the current <see cref="RequestLocalizationOptions"/> that
/// might set from other modules in Orchard Core pipeline.
/// </remarks>
public class LocalizationOptionsUpdater
{
    private readonly bool _useUserOverride;
    private readonly RequestLocalizationOptions _options;

    /// <summary>
    /// Initializes a new instance of a <see cref="LocalizationOptionsUpdater"/>.
    /// </summary>
    /// <param name="options">The <see cref="RequestLocalizationOptions"/>.</param>
    /// <param name="ignoreSystemSettings">Boolean value to indicate whether to ignore system settings or not.</param>
    public LocalizationOptionsUpdater(RequestLocalizationOptions options, bool ignoreSystemSettings)
    {
        _options = options;
        _useUserOverride = !ignoreSystemSettings;
    }

    /// <summary>
    /// Updates the supported culture set.
    /// </summary>
    /// <param name="cultures">The cultures to be supported.</param>
    public LocalizationOptionsUpdater AddSupportedCultures(params string[] cultures)
    {
        var supportedCultures = new List<CultureInfo>();
        foreach (var culture in cultures)
        {
            supportedCultures.Add(new CultureInfo(culture, _useUserOverride));
        }

        _options.SupportedCultures = supportedCultures;

        return this;
    }

    /// <summary>
    /// Updates the supported UI culture set.
    /// </summary>
    /// <param name="uiCultures">The UI cultures to be supported.</param>
    public LocalizationOptionsUpdater AddSupportedUICultures(params string[] uiCultures)
    {
        var supportedUICultures = new List<CultureInfo>();
        foreach (var culture in uiCultures)
        {
            supportedUICultures.Add(new CultureInfo(culture, _useUserOverride));
        }

        _options.SupportedUICultures = supportedUICultures;

        return this;
    }

    /// <summary>
    /// Updates the default culture.
    /// </summary>
    /// <param name="defaultCulture">The default culture.</param>
    public LocalizationOptionsUpdater SetDefaultCulture(string defaultCulture)
    {
        _options.DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, _useUserOverride));

        return this;
    }
}
