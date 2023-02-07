using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Localization;

/// <summary>
/// Represents options for the <see cref="RequestLocalizationMiddleware"/> that extends <see cref="RequestLocalizationOptions"/>.
/// </summary>
public class OrchardCoreRequestLocalizationOptions : RequestLocalizationOptions
{
    private readonly bool _useUserOverride;

    /// <summary>
    /// Creates a new <see cref="OrchardCoreRequestLocalizationOptions"/> with default values.
    /// </summary>
    public OrchardCoreRequestLocalizationOptions() : this(ignoreSystemSettings: false)
    {
    }

    /// <summary>
    /// Creates a new <see cref="OrchardCoreRequestLocalizationOptions"/> with default values and the ability to ignore system settings.
    /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not.</param>
    /// </summary>
    public OrchardCoreRequestLocalizationOptions(bool ignoreSystemSettings) : base()
    {
        _useUserOverride = !ignoreSystemSettings;
    }

    /// <summary>
    /// Initializes the options properties from a provided <see cref="RequestLocalizationOptions"/> instance.
    /// <param name="requestLocalizationOptions">The provided <see cref="RequestLocalizationOptions"/>.</param>
    /// </summary>
    public OrchardCoreRequestLocalizationOptions WithRequestLocalizationOptions(RequestLocalizationOptions requestLocalizationOptions)
    {
        if (requestLocalizationOptions is null)
        {
            throw new ArgumentNullException(nameof(requestLocalizationOptions));
        }

        ApplyCurrentCultureToResponseHeaders = requestLocalizationOptions.ApplyCurrentCultureToResponseHeaders;
        RequestCultureProviders = requestLocalizationOptions.RequestCultureProviders;
        FallBackToParentCultures = requestLocalizationOptions.FallBackToParentCultures;
        FallBackToParentUICultures = requestLocalizationOptions.FallBackToParentUICultures;

        return this;
    }

    /// <inheritdoc/>
    public new OrchardCoreRequestLocalizationOptions AddSupportedCultures(params string[] cultures)
    {
        var supportedCultures = new List<CultureInfo>();

        foreach (var culture in cultures)
        {
            supportedCultures.Add(new CultureInfo(culture, _useUserOverride));
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
            supportedUICultures.Add(new CultureInfo(culture, _useUserOverride));
        }

        SupportedUICultures = supportedUICultures;

        return this;
    }

    /// <inheritdoc/>
    public new OrchardCoreRequestLocalizationOptions SetDefaultCulture(string defaultCulture)
    {
        DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, _useUserOverride));

        return this;
    }
}
