using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace Microsoft.AspNetCore.Builder;

public class RequestLocalizationOptionsBuilder
{
    private readonly bool _useUserOverride;

    public RequestLocalizationOptionsBuilder(RequestLocalizationOptions requestLocalizationOptions, bool useUserOverride)
    {
        Options = requestLocalizationOptions ?? throw new ArgumentNullException(nameof(requestLocalizationOptions));
        _useUserOverride = useUserOverride;
    }

    public RequestLocalizationOptions Options { init;  get; }

    public RequestLocalizationOptionsBuilder AddSupportedCultures(params string[] cultures)
    {
        var supportedCultures = new List<CultureInfo>(cultures.Length);

        foreach (var culture in cultures)
        {
            supportedCultures.Add(new CultureInfo(culture, _useUserOverride));
        }

        Options.SupportedCultures = supportedCultures;

        return this;
    }

    public RequestLocalizationOptionsBuilder AddSupportedUICultures(params string[] uiCultures)
    {
        var supportedUICultures = new List<CultureInfo>(uiCultures.Length);
        foreach (var culture in uiCultures)
        {
            supportedUICultures.Add(new CultureInfo(culture, _useUserOverride));
        }

        Options.SupportedUICultures = supportedUICultures;

        return this;
    }

    public RequestLocalizationOptionsBuilder SetDefaultCulture(string defaultCulture)
    {
        Options.DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, _useUserOverride));

        return this;
    }
}
