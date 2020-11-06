using System;
using System.Globalization;

namespace OrchardCore.Localization
{
    public sealed class CultureScope : IDisposable
    {
        public CultureScope(CultureInfo culture)
        {
            Culture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public CultureInfo Culture { get; private set; }

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = Culture;
        }
    }
}
