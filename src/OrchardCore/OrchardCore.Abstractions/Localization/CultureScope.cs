using System;
using System.Globalization;

namespace OrchardCore.Localization
{
    public sealed class CultureScope : IDisposable
    {
        private CultureScope(CultureInfo culture)
        {
            Culture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public CultureInfo Culture { get; private set; }

        public static CultureScope Create(CultureInfo culture) => new CultureScope(culture);

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = Culture;
        }
    }
}
