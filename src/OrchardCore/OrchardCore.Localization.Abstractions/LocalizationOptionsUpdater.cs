using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Localization;

public class LocalizationOptionsUpdater
{
    private readonly bool _useUserOverride;
    private readonly RequestLocalizationOptions _options;

    public LocalizationOptionsUpdater(RequestLocalizationOptions options, bool ignoreSystemSettings)
    {
        _options = options;
        _useUserOverride = !ignoreSystemSettings;
    }

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

    public LocalizationOptionsUpdater SetDefaultCulture(string defaultCulture)
    {
        _options.DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, _useUserOverride));

        return this;
    }
}
