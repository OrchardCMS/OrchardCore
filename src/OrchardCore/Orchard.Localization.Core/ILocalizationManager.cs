using Orchard.Localization.Abstractions;
using System.Globalization;

namespace Orchard.Localization.Core
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(CultureInfo culture);
    }
}