using System;
using System.Globalization;

namespace OrchardCore.Localization
{
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

        public static CultureScope Create(string culture, string uiCulture)
            => Create(CultureInfo.GetCultureInfo(culture), CultureInfo.GetCultureInfo(uiCulture));

        public static CultureScope Create(CultureInfo culture) => Create(culture, culture);

        public static CultureScope Create(CultureInfo culture, CultureInfo uiCulture) => new CultureScope(culture, uiCulture);

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
}
