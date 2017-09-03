using System.Globalization;

namespace Orchard.Localization
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);
    }
}