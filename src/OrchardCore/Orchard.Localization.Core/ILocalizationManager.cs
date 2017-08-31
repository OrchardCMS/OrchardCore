using System.Globalization;
using Orchard.Localization.Abstractions;

namespace Orchard.Localization.Core
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);
    }
}