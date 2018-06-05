using System.Globalization;

namespace OrchardCore.Localization.Services
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);
    }
}