using System;
using System.Globalization;

namespace OrchardCore.Localization;

public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;

    private CultureScope(string culture, string uiCulture, CultureSettings cultureSettings)
    {
        var useUserOveride = cultureSettings == CultureSettings.User;
        Culture = new CultureInfo(culture, useUserOveride);
        UICulture = new CultureInfo(uiCulture, useUserOveride);
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;

        SetCultures(Culture, UICulture);
    }

    public CultureInfo Culture { get; }

    public CultureInfo UICulture { get; }

    public static CultureScope Create(string culture, CultureSettings cultureSettings = CultureSettings.Default)
        => Create(culture, culture, cultureSettings);

    public static CultureScope Create(string culture, string uiCulture, CultureSettings cultureSettings = CultureSettings.Default)
        => new(culture, uiCulture, cultureSettings);

    public static CultureScope Create(CultureInfo culture, CultureSettings cultureSettings = CultureSettings.Default)
        => Create(culture, culture, cultureSettings);

    public static CultureScope Create(CultureInfo culture, CultureInfo uiCulture, CultureSettings cultureSettings = CultureSettings.Default)
        => new(culture.Name, uiCulture.Name, cultureSettings);

    public void Dispose()
    {
        SetCultures(_originalCulture, _originalUICulture);
    }

    private static void SetCultures(CultureInfo culture, CultureInfo uiCulture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }
}
