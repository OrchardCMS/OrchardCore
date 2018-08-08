using System.Globalization;

namespace OrchardCore.Localization
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);
    }
}