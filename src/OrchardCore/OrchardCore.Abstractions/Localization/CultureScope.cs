using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Localization;

public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _originalUICulture;

    private CultureScope(CultureInfo culture, CultureInfo uiCulture)
    {
        Culture = culture;
        UICulture = uiCulture;
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUICulture = CultureInfo.CurrentUICulture;

        SetCultures(culture, uiCulture);
    }

    public CultureInfo Culture { get; }

    public CultureInfo UICulture { get; }

    public static CultureScope Create(string culture) => Create(culture, culture);

    public static CultureScope Create(string culture, string uiCulture) => CreateInternal(culture, uiCulture);

    public static CultureScope Create(CultureInfo culture) => Create(culture, culture);

    public static CultureScope Create(CultureInfo culture, CultureInfo uiCulture) => CreateInternal(culture.Name, uiCulture.Name);

    public void Dispose()
    {
        SetCultures(_originalCulture, _originalUICulture);
    }

    private static void SetCultures(CultureInfo culture, CultureInfo uiCulture)
    {
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = uiCulture;
    }

    private static CultureScope CreateInternal(string culture, string uiCulture)
    {
        using var scope = ShellScope.Current;
        var useUserSelectedCultureSettings = scope.ServiceProvider.GetService<ILocalizationService>()
            .GetCultureSettingsAsync().GetAwaiter().GetResult() == CultureSettings.User;

        return new CultureScope(
            new CultureInfo(culture, useUserSelectedCultureSettings),
            new CultureInfo(uiCulture, useUserSelectedCultureSettings));
    }
}
