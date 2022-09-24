using System;
using System.Globalization;

namespace OrchardCore.Localization
{
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUICulture;

        private CultureScope(string culture, string uiCulture, bool useUserOverride)
        {
            Culture = new CultureInfo(culture, useUserOverride);
            UICulture = new CultureInfo(uiCulture, useUserOverride);
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUICulture = CultureInfo.CurrentUICulture;

            SetCultures(Culture, UICulture);
        }

        public CultureInfo Culture { get; }

        public CultureInfo UICulture { get; }

        public static CultureScope Create(string culture, bool useUserOverride = true)
            => Create(culture, culture, useUserOverride);

        public static CultureScope Create(string culture, string uiCulture, bool useUserOverride = true)
            => new(culture, uiCulture, useUserOverride);

        public static CultureScope Create(CultureInfo culture, bool useUserOverride = true)
            => Create(culture, culture, useUserOverride);

        public static CultureScope Create(CultureInfo culture, CultureInfo uiCulture, bool useUserOverride = true)
            => new(culture.Name, uiCulture.Name, useUserOverride);

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
