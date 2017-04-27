using Orchard.Localization.Abstractions;

namespace Orchard.Localization.Core
{
    public interface ILocalizationManager
    {
        CultureDictionary GetDictionary(string cultureName);
    }
}