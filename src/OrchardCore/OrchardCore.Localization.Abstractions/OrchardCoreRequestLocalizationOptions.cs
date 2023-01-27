using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents an options for the <see cref="RequestLocalizationMiddleware"/> that extends <see cref="RequestLocalizationOptions"/>.
/// </summary>
public class OrchardCoreRequestLocalizationOptions : RequestLocalizationOptions
{
    /// <summary>
    /// Creates a new <see cref="OrchardCoreRequestLocalizationOptions"/> with default values.
    /// </summary>
    public OrchardCoreRequestLocalizationOptions()
    {
    }

    /// <summary>
    /// Creates a new <see cref="OrchardCoreRequestLocalizationOptions"/> with default values and ability to ignore system settings.
    /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not.</param>
    /// </summary>
    [Obsolete("This constrcutor has been deprecated, it will be removed in the upcoming major release.", error: true)]
    public OrchardCoreRequestLocalizationOptions(bool ignoreSystemSettings)
    {
    }

    /// <summary>
    /// Gets or sets whether to ignore the system culture settings or not.
    /// </summary>
    public bool IgnoreSystemSettings { get; set; }

    /// <inheritdoc/>
    public new OrchardCoreRequestLocalizationOptions AddSupportedCultures(params string[] cultures)
    {
        var supportedCultures = new List<CultureInfo>();

        foreach (var culture in cultures)
        {
            supportedCultures.Add(new CultureInfo(culture, !IgnoreSystemSettings));
        }

        SupportedCultures = supportedCultures;

        return this;
    }

    /// <inheritdoc/>
    public new OrchardCoreRequestLocalizationOptions AddSupportedUICultures(params string[] uiCultures)
    {
        var supportedUICultures = new List<CultureInfo>();
        foreach (var culture in uiCultures)
        {
            supportedUICultures.Add(new CultureInfo(culture, !IgnoreSystemSettings));
        }

        SupportedUICultures = supportedUICultures;

        return this;
    }

    /// <inheritdoc/>
    public new OrchardCoreRequestLocalizationOptions SetDefaultCulture(string defaultCulture)
    {
        DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, !IgnoreSystemSettings));

        return this;
    }
}
