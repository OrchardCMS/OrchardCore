using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
        _options.SupportedCultures = cultures
            .Select(culture => new CultureInfo(culture, _useUserOverride))
            .ToList();

        return this;
    }

    public LocalizationOptionsUpdater AddSupportedUICultures(params string[] uiCultures)
    {
        _options.SupportedUICultures = uiCultures
            .Select(culture => new CultureInfo(culture, _useUserOverride))
            .ToList();

        return this;
    }

    public LocalizationOptionsUpdater SetDefaultCulture(string defaultCulture)
    {
        _options.DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, _useUserOverride));

        return this;
    }
}
