using System;
using System.Globalization;

namespace OrchardCore.Localization
{
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _currentCulture;

        public CultureScope(CultureInfo culture)
        {
            _currentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentUICulture = _currentCulture;
        }
    }
}
