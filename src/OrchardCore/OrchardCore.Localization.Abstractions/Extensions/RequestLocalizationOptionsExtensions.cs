using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for the <see cref="RequestLocalizationOptions"/>.
/// </summary>
public static class RequestLocalizationOptionsExtensions
{
    /// <summary>
    /// Adds the set of the supported cultures by the application.
    /// </summary>
    /// <param name="requestLocalizationOptions">The <see cref="RequestLocalizationOptions"/>.</param>
    /// <param name="cultures">The cultures to be added.</param>
    /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not. Defaults to <c>false</c>.</param>
    /// <returns>The <see cref="RequestLocalizationOptions"/>.</returns>
    public static RequestLocalizationOptions AddSupportedCultures(
        this RequestLocalizationOptions requestLocalizationOptions,
        string[] cultures,
        bool ignoreSystemSettings = false)
    {
        if (requestLocalizationOptions == null)
        {
            throw new ArgumentNullException(nameof(requestLocalizationOptions));
        }

        if (cultures is null)
        {
            throw new ArgumentNullException(nameof(cultures));
        }

        var supportedCultures = new List<CultureInfo>(cultures.Length);

        foreach (var culture in cultures)
        {
            supportedCultures.Add(new CultureInfo(culture, !ignoreSystemSettings));
        }

        requestLocalizationOptions.SupportedCultures = supportedCultures;

        return requestLocalizationOptions;
    }

    /// <summary>
    /// Adds the set of the supported UI cultures by the application.
    /// </summary>
    /// <param name="requestLocalizationOptions">The <see cref="RequestLocalizationOptions"/>.</param>
    /// <param name="cultures">The cultures to be added.</param>
    /// <param name="ignoreSystemSettings">Whether to ignore the system culture settings or not. Defaults to <c>false</c>.</param>
    /// <returns>The <see cref="RequestLocalizationOptions"/>.</returns>
    public static RequestLocalizationOptions AddSupportedUICultures(
        this RequestLocalizationOptions requestLocalizationOptions,
        string[] cultures,
        bool ignoreSystemSettings = false)
    {
        if (requestLocalizationOptions == null)
        {
            throw new ArgumentNullException(nameof(requestLocalizationOptions));
        }

        if (cultures is null)
        {
            throw new ArgumentNullException(nameof(cultures));
        }

        var supportedUICultures = new List<CultureInfo>(cultures.Length);

        foreach (var culture in cultures)
        {
            supportedUICultures.Add(new CultureInfo(culture, !ignoreSystemSettings));
        }

        requestLocalizationOptions.SupportedUICultures = supportedUICultures;

        return requestLocalizationOptions;
    }
}
